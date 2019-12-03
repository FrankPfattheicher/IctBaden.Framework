using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace IctBaden.Framework.Network
{
    public abstract class SimpleHttpServer
    {
        public int Port { get; private set; }

        private Thread _runner;
        private bool _cancelRunner;
        private bool _runnerCanceled;
        private Socket _listener;
        private ManualResetEvent _clientAccepted;

        public delegate void CommandLineHandler(Socket client, string commandLine);
        public delegate void ConnectionHandler(Socket client);

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public List<Socket> Clients { get; private set; }

        public int Connections => Clients.Count;

        public virtual void HandleGetRequest(SimpleHttpProcessor processor)
        {
        }

        public virtual void HandlePostRequest(SimpleHttpProcessor processor, StreamReader inputData)
        {
        }

        protected SimpleHttpServer(int tcpPort)
        {
            Clients = new List<Socket>();
            Port = tcpPort;

            _runner = new Thread(RunnerDoWork);
        }

        private void RunnerDoWork()
        {
            Trace.TraceInformation("SimpleHttpServer.RunnerDoWork()");
            try
            {
                if (_listener == null)
                    return;
                _listener.Listen(10);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                return;
            }
            Trace.TraceInformation("SimpleHttpServer started.");
            _clientAccepted.Set();
            while (!_cancelRunner && (_listener != null) && _listener.IsBound)
            {
                try
                {
                    if (_clientAccepted.WaitOne(1000, false)) // use this version for Windows 2000 compatibility
                    {
                        _clientAccepted.Reset();
                        _listener?.BeginAccept(AcceptClient, null);
                    }
                }
                catch (SocketException ex)
                {
                    if ((ex.SocketErrorCode != SocketError.Interrupted) &&
                        (ex.SocketErrorCode != SocketError.ConnectionReset))
                    {
                        Trace.TraceError(ex.Message);
                    }
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.Message);
                    break;
                }
            }

            Disconnect();
            _runnerCanceled = true;
            Trace.TraceInformation("SimpleHttpServer terminated.");
        }

        private void AcceptClient(IAsyncResult ar)
        {
            _clientAccepted.Set();
            if (_listener == null)
                return;

            try
            {
                var client = _listener.EndAccept(ar);
                var p = new ParameterizedThreadStart(Handler);
                var t = new Thread(p);
                t.Start(client);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public bool Start()
        {
            try
            {
                Trace.TraceInformation("SimpleHttpServer.Start()");
                _clientAccepted = new ManualResetEvent(false);
                if (_listener == null)
                {
                    _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                    _listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    var localEp = new IPEndPoint(0, Port);
                    _listener.Bind(localEp);
                }

                _cancelRunner = false;
                _runnerCanceled = false;
                _runner.Start();
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                return false;
            }
            return true;
        }

        public void Reset()
        {
            Trace.TraceInformation("SimpleHttpServer.Reset()");
            Terminate(false);

            var wait = 30;
            while ((!_runnerCanceled) && (wait > 0))
            {
                Thread.Sleep(1000);
                wait--;
            }

            Debug.Assert(_runner.ThreadState == System.Threading.ThreadState.Stopped, "Should be stopped here.");

            Cancel();
            _runner.Join(TimeSpan.FromSeconds(10));

            _runner = new Thread(RunnerDoWork);

            if (!Start())
            {
                Trace.TraceError("SimpleHttpServer: FATAL ERROR: Restart failed");
            }
        }

        // ReSharper disable once MemberCanBeProtected.Global
        public void Terminate()
        {
            // ReSharper disable once IntroduceOptionalParameters.Global
            Terminate(true);
        }
        public void Terminate(bool disconnectClients)
        {
            try
            {
                Cancel();
                if (disconnectClients)
                {
                    DisconnectAllClients();
                }

                var list = _listener;
                _listener = null;

                if (list == null)
                    return;

                list.Close();
                list.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void Cancel()
        {
            try
            {
                _cancelRunner = true;
                Disconnect();
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
        }

        private void Disconnect()
        {
            if (_listener == null) return;

            if (_listener.Connected)
            {
                _listener.Disconnect(true);
            }
            _listener.Close();
        }

        public void DisconnectAllClients()
        {
            try
            {
                lock (Clients)
                {
                    foreach (var cli in Clients)
                    {
                        try
                        {
                            cli.Shutdown(SocketShutdown.Both);
                            cli.Close();
                        }
                        catch (Exception)
                        {
                            // ignore errors
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
            }
        }


        private void Handler(object param)
        {
            var client = (Socket)param;
            lock (Clients)
            {
                Clients.Add(client);
            }

            var processor = new SimpleHttpProcessor(client, this);
            processor.Process();

            try
            {
                client.Shutdown(SocketShutdown.Both);
                client.Disconnect(false);
            }
            catch (SocketException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            catch (PlatformNotSupportedException)
            {
            }

            lock (Clients)
            {
                Clients.Remove(client);
            }
        }

    }
}
