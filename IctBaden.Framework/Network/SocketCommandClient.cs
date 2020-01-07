using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace IctBaden.Framework.Network
{
    /// <summary>
    /// 
    /// </summary>
    public class SocketCommandClient : IDisposable
    {
        private readonly string _commandHost;
        private readonly int _commandPort;
        private readonly Action<string> _receiveDataHandler;
        private Socket _clientSocket;
        private readonly System.Threading.Timer _pollReceiveData;
        private bool _handlingCommand;

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string LastResult { get; private set; } = string.Empty;

        /// <summary>
        /// Milliseconds
        /// </summary>
        public int ConnectTimeout  { get; set; } = 5000;

        public SocketCommandClient(string host, int port, Action<string> rxHandler)
        {
            _commandHost = host;
            _commandPort = port;
            _receiveDataHandler = rxHandler;
            _pollReceiveData = new System.Threading.Timer(ReceiveData, this, 100, 100);
        }

        #region IDisposable Members

        public void Dispose()
        {
            Disconnect();
            _pollReceiveData.Dispose();
        }

        #endregion

        public bool Connect()
        {
            LastResult = string.Empty;

            if (IsConnected)
            {
                return true;
            }

            if (string.IsNullOrEmpty(_commandHost))
            {
                LastResult = "Host missing";
                return false;
            }

            try
            {
                var hostAddresses = Dns.GetHostAddresses(_commandHost);

                // create an end-point for the first address...
                IPEndPoint endPoint = null;
                foreach (var hostAddress in hostAddresses)
                {
                    if (hostAddress.AddressFamily == AddressFamily.InterNetwork)
                        endPoint = new IPEndPoint(hostAddress, _commandPort);
                }

                if (endPoint == null)
                {
                    LastResult = "Could not resolve " + _commandHost;
                    return false;
                }

                _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                //clientSocket.Connect(endPoint);
                var result = _clientSocket.BeginConnect(endPoint, null, null);
                var success = result.AsyncWaitHandle.WaitOne(ConnectTimeout, true);
                if (!success)
                {
                    LastResult = "Connect timeout";
                    _clientSocket.Close(); // NOTE, MUST CLOSE THE SOCKET
                    return false;
                }
                _clientSocket.EndConnect(result);
            }
            catch (Exception ex)
            {
                LastResult = ex.Message;
                return false;
            }

            return IsConnected;
        }

        public bool IsConnected => (_clientSocket != null) && _clientSocket.Connected;

        public void Disconnect()
        {
            var disconnectSocket = _clientSocket;
            _clientSocket = null;

            if (disconnectSocket != null)
            {
                try
                {
                    if (disconnectSocket.Connected)
                    {
                        disconnectSocket.Disconnect(true);
                    }

                    disconnectSocket.Close();
                    disconnectSocket.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        public void SetReceiveTimeout(TimeSpan timeout)
        {
            SetReceiveTimeout((int)timeout.TotalMilliseconds);
        }
        public void SetReceiveTimeout(int milliSeconds)
        {
            if (_clientSocket == null)
                return;

            _clientSocket.ReceiveTimeout = milliSeconds;
        }

        private void ReceiveData(object state)
        {
            var receiveSocket = _clientSocket;

            if (receiveSocket == null)
                return;
            if (!receiveSocket.Connected)
                return;
            if (_handlingCommand)
                return;

            try
            {
                if (!receiveSocket.Poll(100, SelectMode.SelectRead))
                    return;

                if (_clientSocket == null)
                    return;

                var result = new byte[10240];
                var rxCount = receiveSocket.Receive(result, 0, result.Length, SocketFlags.None);
                _receiveDataHandler(Encoding.Default.GetString(result, 0, rxCount));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public bool SendCommand(string cmd)
        {
            var sendSocket = _clientSocket;

            var data = Encoding.Default.GetBytes(cmd);
            var sent = 0;

            if (sendSocket != null)
            {
                try
                {
                    sent = sendSocket.Send(data);
                }
                catch (Exception)
                {
                    sent = 0;
                }
            }

            return sent == data.Length;
        }

        public string DoCommand(string cmd)
        {
            for (var retry = 0; retry < 3; retry++)
            {
                if (!Connect())
                    continue;

                try
                {
                    var result = new byte[10240];
                    int rxCount;
                    if (_clientSocket.Poll(100, SelectMode.SelectRead))
                    {
                        rxCount = _clientSocket.Receive(result, 0, result.Length, SocketFlags.None);
                        _receiveDataHandler(Encoding.Default.GetString(result, 0, rxCount));
                    }

                    _handlingCommand = true;
                    _clientSocket.Send(Encoding.Default.GetBytes(cmd));
                    Thread.Sleep(5);

                    rxCount = _clientSocket.Receive(result, 0, result.Length, SocketFlags.None);
                    var rxData = Encoding.Default.GetString(result, 0, rxCount);
                    _handlingCommand = false;
                    return rxData;
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch
                // ReSharper restore EmptyGeneralCatchClause
                {
                }
            }

            return string.Empty;
        }

    }
}
