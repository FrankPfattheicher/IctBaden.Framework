// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

using System.Diagnostics.CodeAnalysis;
using IctBaden.Framework.AppUtils;

namespace IctBaden.Framework.Network;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

using ThreadState = System.Threading.ThreadState;

[SuppressMessage("Design", "MA0046:Use EventHandler<T> to declare events")]
public sealed class SocketCommandLineServer : IDisposable
{
    private readonly int _port;
    private readonly bool _publicReachable;
    private Thread _runner;
    private bool _cancelRunner;
    private bool _runnerCanceled;
    private Socket? _listener;
    private ManualResetEvent _clientAccepted;
    private bool _disposed;

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _listener?.Dispose();
        _clientAccepted.Dispose();
    }

    // ReSharper disable once UnusedMember.Local
    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }

    public delegate void CommandLineHandler(Socket client, string commandLine);
    public delegate void ConnectionHandler(Socket client);

    public event CommandLineHandler? HandleCommand;
    public event ConnectionHandler? ClientConnected;
    public event ConnectionHandler? ClientDisconnected;

#pragma warning disable MA0016
    public List<Socket> Clients { get; private set; }
#pragma warning restore MA0016

    public int Connections => Clients.Count;

    public string? StartOfCommand { get; set; }
#pragma warning disable MA0016
    public List<string> Eoc { get; set; }
#pragma warning restore MA0016
    public Encoding UseEncoding { get; set; }
    public bool HandleEmptyCommands { get; set; }

    public SocketCommandLineServer(int tcpPort)
        // ReSharper disable once IntroduceOptionalParameters.Global
        : this(tcpPort, true)
    {
    }
    public SocketCommandLineServer(int tcpPort, bool publicReachable)
    {
        Clients = [];
        _port = tcpPort;
        _publicReachable = publicReachable;
        Eoc = ["\r", "\n"];
        UseEncoding = Encoding.UTF8;
        HandleEmptyCommands = false;

        _runner = new Thread(RunnerDoWork);
        _clientAccepted = new ManualResetEvent(false);
    }

    private void RunnerDoWork()
    {
        Trace.TraceInformation("SocketCommandLineServer.RunnerDoWork()");
        try
        {
            if (_listener == null)
                return;
            _listener.Listen(100);
        }
        catch (Exception ex)
        {
            Trace.TraceError(ex.Message);
            return;
        }
        Trace.TraceInformation("SocketCommandLineServer started.");
        _clientAccepted.Set();
        while (!_cancelRunner && _listener is { IsBound: true })
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
            Trace.TraceInformation($"SocketCommandLineServer.Start({_port})");
            _clientAccepted.Dispose();
            _clientAccepted = new ManualResetEvent(false);
            if (_listener == null)
            {
                _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                if (SystemInfo.Platform == Platform.Windows)
                {
                    _listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                }
                _listener.LingerState = new LingerOption(false, 0);
                _listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                var localEp = new IPEndPoint(_publicReachable ? IPAddress.Any : IPAddress.Loopback, _port);
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


    [SuppressMessage("Design", "MA0051:Method is too long")]
    private void Handler(object? param)
    {
        if (param is not Socket client) return;
            
        if (SystemInfo.Platform == Platform.Windows)
        {
            client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, optionValue: true);
        }
        client.LingerState = new LingerOption(enable: false, 0);
        client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, optionValue: true);
        lock (Clients)
        {
            Clients.Add(client);
        }

        ClientConnected?.Invoke(client);

        var rxString = string.Empty;
        while (client is { IsBound: true, Connected: true })
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
                        if (!rxString.Contains(eoc, StringComparison.InvariantCulture)) 
                            continue;

                        rxString = rxString.Replace(eoc, string.Empty, StringComparison.OrdinalIgnoreCase);
                        if (!string.IsNullOrEmpty(StartOfCommand))
                        {
                            var start = rxString.IndexOf(StartOfCommand, StringComparison.Ordinal);
                            rxString = start >= 0 ? rxString.Substring(start + 1) : string.Empty;
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
            client.Disconnect(reuseSocket: false);
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