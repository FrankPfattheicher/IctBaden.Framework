using System;
using System.Collections.Generic;
using System.Threading;
using IctBaden.Framework.Network;
using IctBaden.Framework.Timer;
using Xunit;

namespace IctBaden.Framework.Test.Network
{
    [CollectionDefinition("TcpClientServerTests", DisableParallelization = true)]
    public class SocketCmdLineTests : IDisposable
    {
        private readonly int _testServerPort;
        private readonly SocketCommandLineServer _server;

        public SocketCmdLineTests()
        {
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

    }
}
