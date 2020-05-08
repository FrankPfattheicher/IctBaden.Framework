using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        private SocketCommandClient _client;

        private CancellationTokenSource _cancel;
        private Task _task;

        public SocketCmdLineTests()
        {
            _testServerPort = NetworkInfo.GetFreeLocalTcpPort();
            _server = new SocketCommandLineServer(_testServerPort);
        }

        public void Dispose()
        {
            _cancel?.Cancel();
            if (_task != null)
            {
                if (_task.Wait(1000))
                {
                    _task?.Dispose();
                }
                _task = null;
            }

            _client?.Dispose();
            _client = null;
            
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

            _client = new SocketCommandClient("localhost", _testServerPort, s => { });
            var connected = _client.Connect();
            Assert.True(connected, "LastResult: " + _client.LastResult);
            Thread.Sleep(100);

            Assert.True(reportConnected);
            Assert.Single(_server.Clients);

            _client.Dispose();
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
            _client = new SocketCommandClient("localhost", _testServerPort, s => { });
            var connected = _client.Connect();
            Assert.True(connected, "Could not connect to server: " + _client.LastResult);

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
            _client = new SocketCommandClient("localhost", _testServerPort, s => { });

            var connected = _client.Connect();
            Assert.False(connected, "LastResult: " + _client.LastResult);
        }
        
        [Fact]
        public void DoCommandShouldAutoConnectToServer()
        {
            var started = _server.Start();
            Assert.True(started, "Could not start server");

            _server.HandleCommand += (socket, line) => socket.Send(Encoding.ASCII.GetBytes(line)); 
            
            _client = new SocketCommandClient("localhost", _testServerPort, s => { });

            const string cmd = "TEST*TEST";
            var response = _client.DoCommand(cmd + _server.Eoc.First());
            Assert.True(_client.IsConnected);
            Assert.Equal(cmd, response);
        }

        [Fact]
        public void DisconnectCommandShouldSucceed()
        {
            var started = _server.Start();
            Assert.True(started, "Could not start server");

            _client = new SocketCommandClient("localhost", _testServerPort, s => { });

            var connected = _client.Connect();
            Assert.True(connected, "LastResult: " + _client.LastResult);

            _client.Disconnect();
            Assert.False(_client.IsConnected);
        }

        [Fact]
        public void IncompleteCommandShouldNotTimeoutIfNotSpecified()
        {
            var started = _server.Start();
            Assert.True(started, "Could not start server");

            _client = new SocketCommandClient("localhost", _testServerPort, s => { })
            {
                CommandRetryCount = 0
            };

            var connected = _client.Connect();
            Assert.True(connected, "LastResult: " + _client.LastResult);

            _cancel = new CancellationTokenSource();
            _task = Task.Run(() => _client.DoCommand("TEST"), _cancel.Token);
            var completedInTime = Task.WaitAll(new Task[] { _task }, 5000, _cancel.Token);
            _cancel.Cancel();

            Assert.False(completedInTime, "Should NOT timeout");
        }

        [Fact]
        public void IncompleteCommandShouldTimeoutInTimeIfSpecified()
        {
            var started = _server.Start();
            Assert.True(started, "Could not start server");

            _client = new SocketCommandClient("localhost", _testServerPort, s => { })
            {
                ConnectTimeout = 1000, 
                CommandRetryCount = 0
            };

            _client.Connect();
            _client.SetReceiveTimeout(1000);

            _cancel = new CancellationTokenSource();
            _task = Task.Run(() => _client.DoCommand("TEST"), _cancel.Token);
            var completedInTime = Task.WaitAll(new Task[] { _task }, 6000, _cancel.Token);
            _cancel.Cancel();

            Assert.True(completedInTime, "Should timeout");
        }

        
    }
}
