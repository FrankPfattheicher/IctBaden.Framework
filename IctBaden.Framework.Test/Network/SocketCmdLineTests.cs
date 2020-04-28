﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IctBaden.Framework.Network;
using IctBaden.Framework.Timer;
using Xunit;
using Xunit.Abstractions;

namespace IctBaden.Framework.Test.Network
{
    [CollectionDefinition("TcpClientServerTests", DisableParallelization = true)]
    public class SocketCmdLineTests : IDisposable
    {
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly int _testServerPort;
        private readonly SocketCommandLineServer _server;

        public SocketCmdLineTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _testServerPort = NetworkInfo.GetFreeLocalTcpPort();
            _server = new SocketCommandLineServer(_testServerPort);
        }

        public void Dispose()
        {
            _server.Terminate();
        }

        [Fact]
        public void ConnectAndDisposeOneClientShouldBeTrackedByServer()
        {
            var started = _server.Start();
            Assert.True(started, "Could not start server");
            Assert.Empty(_server.Clients);

            var reportConnected = false;
            _server.ClientConnected += socket => { reportConnected = true; };
            var reportDisconnected = false;
            _server.ClientDisconnected += socket => { reportDisconnected = true; };

            var client = new SocketCommandClient("localhost", _testServerPort, s => { });
            var connected = client.Connect();
            Assert.True(connected, "LastResult: " + client.LastResult);
            Thread.Sleep(100);

            Assert.True(reportConnected);
            Assert.Single(_server.Clients);

            client.Dispose();
            Thread.Sleep(100);

            Assert.True(reportDisconnected);
            Assert.Empty(_server.Clients);
        }

        [Fact]
        public void TerminateFromOtherThread()
        {
            var started = _server.Start();
            Assert.True(started, "Could not start server");

            Thread.Sleep(100);
            var client = new SocketCommandClient("localhost", _testServerPort, s => { });
            var connected = client.Connect();
            Assert.True(connected, "Could not connect to server: " + client.LastResult);

            Thread.Sleep(100);
            Assert.Single(_server.Clients);

            var terminator = new Thread(() => _server.Terminate());
            terminator.Start();

            while (terminator.IsAlive)
            {
                Thread.Sleep(100);
            }

            Thread.Sleep(100);
            Assert.Empty(_server.Clients);
        }

        [Fact]
        public void DisconnectFromThisAndOtherThread()
        {
            const int clientCount = 400;
            var started = _server.Start();
            Assert.True(started, "Could not start server");

            Thread.Sleep(100);
            Assert.Empty(_server.Clients);

            var clients1 = new List<SocketCommandClient>();
            for (var cx = 1; cx <= (clientCount / 2); cx++)
            {
                var client = new SocketCommandClient("localhost", _testServerPort, s => { });
                var connected = client.Connect();
                Assert.True(connected, "[1] Could not connect to server: " + client.LastResult);

                clients1.Add(client);
            }
            var clients2 = new List<SocketCommandClient>();
            for (var cx = 1; cx <= (clientCount / 2); cx++)
            {
                var client = new SocketCommandClient("localhost", _testServerPort, s => { });
                var connected = client.Connect();
                Assert.True(connected, "[2] Could not connect to server: " + client.LastResult);

                clients2.Add(client);
            }
            
            Assert.Equal(clientCount / 2, clients1.Count);
            Assert.Equal(clientCount / 2, clients2.Count);
            var waitConnected = new PassiveTimer(TimeSpan.FromSeconds(10));
            while (!waitConnected.Timeout)
            {
                if (_server.Clients.Count == clientCount)
                    break;
                Thread.Sleep(10);
            }
            Assert.Equal(clientCount, _server.Clients.Count);

            // ReSharper disable once IdentifierTypo
            var disconnector = new Thread(() =>
            {
                Thread.Sleep(100);
                while (clients1.Count > 0)
                {
                    clients1[0].Disconnect();
                    clients1.RemoveAt(0);
                    Thread.Sleep(0);
                }
            });
            disconnector.Start();

            Thread.Sleep(100);
            while (clients2.Count > 0)
            {
                clients2[0].Disconnect();
                clients2.RemoveAt(0);
                Thread.Sleep(0);
            }

            while (disconnector.IsAlive)
            {
                Thread.Sleep(10);
            }

            Thread.Sleep(100);
            Assert.Empty(clients1);
            Assert.Empty(clients2);
            Assert.Empty(_server.Clients);
        }

        [Fact]
        public void NextFreeTcpPortShouldNotBeZero()
        {
            var port = NetworkInfo.GetFreeLocalTcpPort();
            Assert.True(port > 0);
        }

        [Fact]
        public void NextFreeUdpPortShouldNotBeZero()
        {
            var port = NetworkInfo.GetFreeLocalUdpPort();
            Assert.True(port > 0);
        }

        
        [Fact]
        public void ClientConnectShouldTimeoutIfServerNotRunning()
        {
            using var client = new SocketCommandClient("localhost", _testServerPort, s => { });

            var connected = client.Connect();
            Assert.False(connected, "LastResult: " + client.LastResult);
        }
        
        [Fact]
        public void IncompleteCommandShouldNotTimeoutIfNotSpecified()
        {
            var started = _server.Start();
            Assert.True(started, "Could not start server");

            using var client = new SocketCommandClient("localhost", _testServerPort, s =>
            {
                _testOutputHelper.WriteLine(s);
            });

            var connected = client.Connect();
            Assert.True(connected, "LastResult: " + client.LastResult);

            var task = Task.Run(() => client.DoCommand("TEST"));
            var completedInTime = Task.WaitAll(new Task[] { task }, TimeSpan.FromSeconds(5));

            Assert.False(completedInTime);
        }

        [Fact]
        public void DoCommandShouldAutoConnectToServer()
        {
            var started = _server.Start();
            Assert.True(started, "Could not start server");

            _server.HandleCommand += (socket, line) => socket.Send(Encoding.ASCII.GetBytes(line)); 
            
            using var client = new SocketCommandClient("localhost", _testServerPort, s =>
            {
                _testOutputHelper.WriteLine(s);
            });

            const string cmd = "TEST*TEST";
            var response = client.DoCommand(cmd + _server.Eoc.First());
            Assert.True(client.IsConnected);
            Assert.Equal(cmd, response);
        }

        [Fact]
        public void DisconnectCommandShouldSucceed()
        {
            var started = _server.Start();
            Assert.True(started, "Could not start server");

            using var client = new SocketCommandClient("localhost", _testServerPort, s =>
            {
                _testOutputHelper.WriteLine(s);
            });

            var connected = client.Connect();
            Assert.True(connected, "LastResult: " + client.LastResult);

            client.Disconnect();
            Assert.False(client.IsConnected);
        }

        [Fact]
        public void IncompleteCommandShouldTimeoutInTimeIfSpecified()
        {
            var started = _server.Start();
            Assert.True(started, "Could not start server");

            var client = new SocketCommandClient("localhost", _testServerPort, s => { });

            try
            {
                client.Connect();
                client.SetReceiveTimeout(TimeSpan.FromSeconds(1));

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var task = Task.Run(() => client.DoCommand("TEST"));
                Task.WaitAll(new Task[] {task}, TimeSpan.FromSeconds(6));

                stopwatch.Stop();
                Assert.True(stopwatch.Elapsed >= TimeSpan.FromSeconds(1));
                Assert.True(stopwatch.Elapsed <= TimeSpan.FromSeconds(5));
            }
            catch (Exception ex)
            {
                Assert.True(false, ex.Message);
            }
            finally
            {
                client.Dispose();
            }
        }

        
    }
}
