using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using IctBaden.Framework.Tron;
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable EventNeverSubscribedTo.Global
// ReSharper disable CommentTypo
// ReSharper disable IntroduceOptionalParameters.Global

namespace IctBaden.Framework.Channels
{
    public class Modem
    {
        private SerialPort _ser;
        private ModemTriggerCollection _trigger;
        private string _lastCommand;

        public string TraceId { get; private set; }
        public string DisplayName { get; private set; }
        public string Port { get; private set; }
        public int Baud { get; private set; }

        public int SignalStrength { get; private set; }
        private string _response;
        private bool _discardReceivedData;
        public string LastError { get; private set; }

        public delegate void ModemResultHandler(object sender, ModemEventArgs e);
        public event ModemResultHandler OnModemResult;

        private Thread _reader;
        private readonly Mutex _modemResult;

        public Modem(string traceId, string modemName, string modemPort, int modemBaud)
        {
            TraceId = traceId;
            DisplayName = modemName;
            Port = modemPort;
            Baud = modemBaud;

            SignalStrength = 0;

            _response = string.Empty;
            _modemResult = new Mutex(false);
        }

        public override string ToString()
        {
            return $"{DisplayName}, {Port}:{Baud}";
        }

        public bool Open()
        {
            if (TronTrace.IsOn(TronTrace.Channel.Modem))
            {
                TronTrace.SetColor(TraceColor.Blue);
                TronTrace.PrintLine($"Modem[{TraceId}] Open({Port}, {Baud})");
            }

            if (_ser != null)
                Close();

            _ser = new SerialPort(Port, Baud, Parity.None, 8, StopBits.One)
            {
                DtrEnable = true,
                RtsEnable = true,
                Encoding = Encoding.ASCII,
                Handshake = Handshake.None,
            };
            if (Environment.OSVersion.Platform != PlatformID.Unix)
            {
                _ser.ReceivedBytesThreshold = 1;
            }
            lock (_response)
            {
                _response = string.Empty;
            }
            _lastCommand = string.Empty;
            _trigger = new ModemTriggerCollection();
            _ser.DataReceived += SerDataReceived;
            try
            {
                _ser.Open();
            }
            catch (IOException e)
            {
                LastError = e.Message;
                TronTrace.PrintLine($"Error={e.Message}");
            }
            catch (UnauthorizedAccessException e)
            {
                LastError = e.Message;
                TronTrace.PrintLine($"Error={e.Message}");
            }
            catch (ArgumentException e)
            {
                LastError = e.Message;
                TronTrace.PrintLine($"Error={e.Message}");
            }
            if (_ser == null || !_ser.IsOpen)
            {
                return false;
            }

            _ser.ReadTimeout = 100;
            _ser.WriteTimeout = 10000;
            _ser.RtsEnable = true;
            _ser.DtrEnable = true;
            _ser.Handshake = Handshake.None;

            if (Environment.OSVersion.Platform != PlatformID.Unix)
            {
                return true;
            }

            // ReSharper disable once RedundantDelegateCreation
            _reader = new Thread(new ThreadStart(delegate
                {
                    while (_ser != null)
                    {
                        try
                        {
                            if (_ser.IsOpen && (_ser.BytesToRead > 0))
                            {
                                _discardReceivedData = false;
                                var data = new byte[_ser.BytesToRead];
                                _ser.Read(data, 0, data.Length);
                                SerDataReceived(Encoding.ASCII.GetString(data));
                            }
                            else
                            {
                                Thread.Sleep(100);
                            }
                        }
                        catch (Exception)
                        {
                            Thread.Sleep(1000);
                        }
                    }
                }));
            _reader.Start();
            return true;
        }

        public void Close()
        {
            var s = _ser;
            _ser = null;

            if (TronTrace.IsOn(TronTrace.Channel.Modem))
            {
                TronTrace.SetColor(TraceColor.Blue);
                TronTrace.PrintLine($"Modem[{TraceId}] Close({((s == null) ? string.Empty : s.PortName)})");
            }

            if (s == null)
                return;

            try
            {
                s.RtsEnable = false;
                s.DtrEnable = false;
                s.Close();
                s.Dispose();
            }
            catch (IOException)
            {
                // ignored
            }
            catch (UnauthorizedAccessException)
            {
                // ignored
            }
        }

        public bool IsOpen => (_ser != null) && _ser.IsOpen;

        private void SerDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var s = _ser;

            if (s == null)
                return;

            if (e.EventType != SerialData.Chars)
                return;

            try
            {
                _discardReceivedData = false;
                var data = s.ReadExisting();
                SerDataReceived(data);
            }
            catch (InvalidOperationException)
            {
            }
        }

        private void SerDataReceived(string data)
        {
            lock (_response)
            {
                _response += data;
            }
            if (TronTrace.IsOn(TronTrace.Channel.Serial))
            {
                TronTrace.PrintRxBuffer(Encoding.ASCII.GetBytes(data));
            }

            foreach (var ch in data)
            {
                if (_discardReceivedData)
                    break;

                lock (_response)
                {
                    if (!char.IsWhiteSpace(ch) || (_response.Length > 0))
                    {
                        if ((_lastCommand.Length > 0) && _response.StartsWith(_lastCommand))
                        {
                            _response = _response.Substring(_lastCommand.Length);
                            _lastCommand = string.Empty;
                        }
                    }
                }

                var result = _trigger.Match(ch);
                if (result.Result != ModemResult.None)
                {
                    Task.Factory.StartNew(() =>
                    {
                        var taken = false;
                        try
                        {
                            taken = _modemResult.WaitOne();

                            lock (_response)
                            {
                                _response = _response.TrimStart();
                                if (_response.StartsWith(result.Text))
                                {
                                    _response = _response.Substring(result.Text.Length);
                                }
                            }

                            FireOnModemResult(result.Result);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                        }
                        finally
                        {
                            if (taken)
                            {
                                _modemResult.ReleaseMutex();
                            }
                        }
                    });
                }
            }
        }

        public bool SendCommand(string command)
        {
            return Send(command, "\r");
        }
        public bool Send(string command, string eoc)
        {
            lock (_response)
            {
                _response = string.Empty;
            }

            if (_ser == null)
            {
                TronTrace.SetColor(TraceColor.Blue);
                TronTrace.PrintLine($"Modem[{TraceId}] SendCommand: Port not open.");
                return false;
            }


            _lastCommand = command;
            if (TronTrace.IsOn(TronTrace.Channel.Modem))
            {
                TronTrace.SetColor(TraceColor.Blue);
                TronTrace.PrintLine($"Modem[{TraceId}] Command: {command}");
            }
            if (!string.IsNullOrEmpty(eoc))
                command += eoc;
            if (TronTrace.IsOn(TronTrace.Channel.Serial))
            {
                TronTrace.PrintTxBuffer(Encoding.ASCII.GetBytes(command));
            }
            try
            {
                _discardReceivedData = true;
                Thread.Sleep(100);  // recommented by SIEMENS
                _ser?.DiscardInBuffer();
                _ser?.Write(command);
            }
            catch (IOException e)
            {
                LastError = e.Message;
                Close();
                return false;
            }
            catch (InvalidOperationException e)
            {
                LastError = e.Message;
                Close();
                return false;
            }
            catch (TimeoutException e)
            {
                LastError = e.Message;
                return false;
            }
            return true;
        }

        public string GetResponse()
        {
            return GetResponse("\r\n");
        }
        public string GetResponse(string terminator)
        {
            var retry = 0;
            while (retry++ < 5)
            {
                lock (_response)
                {
                    if (_response.Contains(terminator))
                        break;
                }
                Thread.Sleep(20);
            }

            string thisResponse;
            lock (_response)
            {
                var endOfResponse = _response.IndexOf(terminator, StringComparison.Ordinal);
                if (endOfResponse == -1)
                {
                    if (TronTrace.IsOn(TronTrace.Channel.Modem))
                    {
                        TronTrace.SetColor(TraceColor.Error);
                        TronTrace.PrintLine("GetResponse failed - expected terminator missing.");
                    }
                    return string.Empty;
                }

                thisResponse = _response.Substring(0, endOfResponse);
                _response = _response.Substring(thisResponse.Length + terminator.Length);
            }

            if (TronTrace.IsOn(TronTrace.Channel.Modem))
            {
                TronTrace.SetColor(TraceColor.Blue);
                TronTrace.PrintLine($"Modem[{TraceId}] Response={thisResponse}");
            }

            return thisResponse;
        }
        public void ResetResponse(string text)
        {
            lock (_response)
            {
                _response = _response.Replace(text, string.Empty);
                if (TronTrace.IsOn(TronTrace.Channel.Modem))
                {
                    TronTrace.SetColor(TraceColor.Blue);
                    TronTrace.PrintLine($"Modem[{TraceId}] ResetResponse {_response}");
                }
            }
        }

        public void ResetResponse()
        {
            lock (_response)
            {
                if (TronTrace.IsOn(TronTrace.Channel.Modem))
                {
                    TronTrace.SetColor(TraceColor.Blue);
                    TronTrace.PrintLine($"Modem[{TraceId}] ResetResponse {_response}");
                }
                _response = string.Empty;
            }
        }

        public void SetUserModemResult(ModemResult result, string pattern)
        {
            _trigger.SetUserTrigger(result, pattern);
        }

        private void FireOnModemResult(ModemResult result)
        {
            if (TronTrace.IsOn(TronTrace.Channel.Modem))
            {
                TronTrace.SetColor(TraceColor.Blue);
                TronTrace.PrintLine($"Modem[{TraceId}] Result: {result}");
            }

            var handler = OnModemResult;
            if (handler == null)
                return;
            handler(this, new ModemEventArgs(result));
            Thread.Sleep(0);
        }
    }
}
