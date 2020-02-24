using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using IctBaden.Framework.AppUtils;
// ReSharper disable UnusedType.Global

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace IctBaden.Framework.Network
{
    public abstract class SimpleHttpServer
    {
        public int Port { get; private set; }

        private Socket _listener;

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
        }

        private void AcceptClient(IAsyncResult ar)
        {
            if (!(ar.AsyncState is Socket listener)) return;

            try
            {
                var client = _listener.EndAccept(ar);
                var p = new ParameterizedThreadStart(Handler);
                var t = new Thread(p);
                t.Start(client);
                
                listener.BeginAccept(AcceptClient, listener);
            }
            catch (ObjectDisposedException)
            {
                // listener terminated
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
                _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                _listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                var localEp = new IPEndPoint(0, Port);
                
                _listener.Bind(localEp);
                _listener.Listen(100);
                
                _listener?.BeginAccept(AcceptClient, _listener);
                return true;
            }
            catch (Exception ex)
            {
                if (ex is Win32Exception native)
                {
                    if (native.NativeErrorCode == 13 && SystemInfo.Platform == Platform.Linux && Port < 1024)
                    {
                        Trace.TraceError("Ports below 1024 are considered 'privileged' and can only be bound to with an equally privileged user (read: root).");
                    }
                }
                Trace.TraceError(ex.Message);
                _listener?.Close();
                _listener?.Dispose();
                return false;
            }
        }

        public void Reset()
        {
            Trace.TraceInformation("SimpleHttpServer.Reset()");
            Terminate(false);

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
                _listener.Close();
                _listener.Dispose();

                if (disconnectClients)
                {
                    DisconnectAllClients();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
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
