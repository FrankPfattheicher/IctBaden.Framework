// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable UnusedMember.Global
namespace IctBaden.Framework.Network
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Net.Sockets;
    using System.Net;
    using System.Threading;

    using ThreadState = System.Threading.ThreadState;

    public class SocketCommandLineServer
    {
        private readonly int _port;
        private Thread _runner;
        private bool _cancelRunner;
        private bool _runnerCanceled;
        private Socket _listener;
        private ManualResetEvent _clientAccepted;

        public delegate void CommandLineHandler(Socket client, string commandLine);
        public delegate void ConnectionHandler(Socket client);

        public event CommandLineHandler HandleCommand;
        public event ConnectionHandler ClientConnected;
        public event ConnectionHandler ClientDisconnected;

        public List<Socket> Clients { get; private set; }

        public int Connections => Clients.Count;

        public string StartOfCommand { get; set; }
        public List<string> Eoc { get; set; }
        public Encoding UseEncoding { get; set; }
        public bool HandleEmptyCommands { get; set; }

        public SocketCommandLineServer(int tcpPort)
        {
            Clients = new List<Socket>();
            _port = tcpPort;
            Eoc = new List<string> {"\r", "\n"};
            UseEncoding = Encoding.UTF8;
            HandleEmptyCommands = false;

            _runner = new Thread(RunnerDoWork);
        }

        private void RunnerDoWork()
        {
            Trace.TraceInformation("SocketCommandLineServer.RunnerDoWork()");
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
            Trace.TraceInformation("SocketCommandLineServer started.");
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
            Trace.TraceInformation("SocketCommandLineServer terminated.");
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
                Trace.TraceInformation("SocketCommandLineServer.Start()");
                _clientAccepted = new ManualResetEvent(false);
                if (_listener == null)
                {
                    _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                    _listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    var localEp = new IPEndPoint(0, _port);
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
            Trace.TraceInformation("SocketCommandLineServer.Reset()");
            Terminate(false);

            var wait = 30;
            while ((!_runnerCanceled) && (wait > 0))
            {
                Thread.Sleep(1000);
                wait--;
            }

            Debug.Assert(_runner.ThreadState == ThreadState.Stopped, "Should be stopped here.");

            Cancel();
            try
            {
                if (_runner.IsAlive)
                {
                    _runner.Join(TimeSpan.FromSeconds(10));
                }
            }
            catch
            {
                // ignore
            }

            _runner = new Thread(RunnerDoWork);

            if(!Start())
            {
                Trace.TraceError("SocketCommandLineServer: FATAL ERROR: Restart failed");
            }
        }

        public void Terminate(bool disconnectClients = true)
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
            if (_listener != null)
            {
                if (_listener.Connected)
                {
                    _listener.Disconnect(true);
                }
                _listener.Close();
            }
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

            ClientConnected?.Invoke(client);

            var rxString = string.Empty;
            //client.ReceiveTimeout = 30000;
            while (client.IsBound && client.Connected)
            {
                try
                {
                    var rxData = new byte[2048];
                    int rxLen;
                    try
                    {
                        rxLen = client.Receive(rxData, 0, rxData.Length, SocketFlags.None); 
                    }
                    catch (SocketException ex)
                    {
                        Debug.WriteLine(ex.Message);
                        break;
                    }
                    if (rxLen == 0)
                    {
                        break;
                    }

                    var rxStr = UseEncoding.GetString(rxData, 0, rxLen);
                    // ReSharper disable once ForCanBeConvertedToForeach
                    for (var cx = 0; cx < rxStr.Length; cx++)
                    {
                        rxString += rxStr[cx];

                        foreach (var eoc in Eoc)
                        {
                            if (rxString.IndexOf(eoc, StringComparison.InvariantCulture) == -1) 
                                continue;

                            rxString = rxString.Replace(eoc, string.Empty);
                            if (!string.IsNullOrEmpty(StartOfCommand))
                            {
                                var start = rxString.IndexOf(StartOfCommand, StringComparison.Ordinal);
                                rxString = (start >= 0) ? rxString.Substring(start + 1) : string.Empty;
                            }
                            if (!string.IsNullOrEmpty(rxString) || HandleEmptyCommands)
                            {
                                if (client.Connected)
                                {
                                    HandleCommand?.Invoke(client, rxString);
                                }
                            }
                            rxString = string.Empty;
                            break;
                        }
                    }
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode != SocketError.ConnectionReset)
                    {
                        Trace.TraceError(ex.Message);
                    }
                    break;
                }
                catch (ObjectDisposedException ex)
                {
                    Trace.TraceError(ex.Message);
                    break;
                }
                catch (PlatformNotSupportedException ex)
                {
                    Trace.TraceError(ex.Message);
                    break;
                }
                catch(ThreadAbortException ex)
                {
                    Trace.TraceError(ex.Message);
                    break;
                }
            }
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

            ClientDisconnected?.Invoke(client);
            lock (Clients)
            {
                Clients.Remove(client);
            }
        }

        public void SendToAllClients(byte[] data)
        {
            lock (Clients)
            {
                foreach (var client in Clients)
                {
                    client.Send(data);
                }
            }
        }
    }
}
