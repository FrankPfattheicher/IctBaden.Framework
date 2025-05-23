﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.IO.Pipes;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using IctBaden.Framework.Timer;

// ReSharper disable CommentTypo
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

// ReSharper disable UnusedMember.Global

namespace IctBaden.Framework.Tron;

public static class TronTrace
{
    // ReSharper disable UnusedMember.Local
    // ReSharper disable InconsistentNaming
    public enum Channel
    {
        Serial = 0x00000001,
        Modem = 0x00000002,
        TelState = 0x00000004,
        TelUser = 0x00000008,
        ModemEx = 0x00000010,
        CAPI = 0x00000020,
        CAPIx = 0x00000040
    }

    // ReSharper restore InconsistentNaming

    private enum TronCommand
    {
        CheckTrace = 2, // none
        Cls = 3, // none
        Print = 4, // char* (Win16), COPYDATASTRUCT* (Win32)
        SetColor = 5, // COLORREF
        PrintChar = 6, // char
        PrintRxByte = 7, // BYTE
        PrintTxByte = 8, // BYTE
        SetTrace = 9, // TRON_Trace...
        Params = 10, // COPYDATASTRUCT* (Win32)
        PrintRxBuffer = 11, // COPYDATASTRUCT* (Win32)  4.80
        PrintTxBuffer = 12 // COPYDATASTRUCT* (Win32)  4.80
    }

    // ReSharper restore UnusedMember.Local

    public static string PipeName { get; set; } = "ICTBaden.tron";
    private static NamedPipeClientStream? _tron;

    public static bool EnableTronWindow { get; set; } = true;

    public static int UdpPort { get; set; } = 1959;
    public static bool UseUdpConnection { get; set; } = true;

    private static readonly PassiveTimer ReConnect = new PassiveTimer(10);


    static TronTrace()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }
        
    public static bool Connect()
    {
        if (_tron != null)
            return Connected;
        if (!EnableTronWindow || !ReConnect.Timeout)
            return false;

        try
        {
            _tron?.Dispose();
            _tron = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            _tron.Connect(10);
        }
        catch (Exception)
        {
            Disconnect();
        }

        ReConnect.Start(TimeSpan.FromSeconds(30));
        return Connected;
    }

    private static void Disconnect()
    {
        try
        {
            _tron?.Dispose();
            _tron = null;
        }
        catch
        {
            // ignore
        }
    }

    public static bool Connected => _tron is { IsConnected: true };

    private static void Send(TronCommand command, byte[]? data)
    {
        // UDP connection - requires TRON V3.0
        if (UseUdpConnection)
        {
            try
            {
                var txLength = 1;
                var txBuffer = new byte[txLength];
                txBuffer[0] = (byte)command;

                if (data != null)
                {
                    txLength = data.Length + 1;
                    txBuffer = new byte[txLength];
                    txBuffer[0] = (byte)command;
                    data.CopyTo(txBuffer, 1);
                }

                using var udp = new UdpClient("127.0.0.1", UdpPort);
                udp.Send(txBuffer, txLength);
                udp.Close();
            }
            catch
            {
                // ignore
            }
            return;
        }


        // Named pipe connection
        if (!Connect() || _tron == null)
            return;

        try
        {
            if (data == null)
            {
                _tron.WriteByte((byte)command);
            }
            else
            {
                var txLength = data.Length + 1;
                var txBuffer = new byte[txLength];
                txBuffer[0] = (byte)command;
                data.CopyTo(txBuffer, 1);
                _tron.Write(txBuffer, 0, txLength);
            }
        }
        catch (Exception)
        {
            Disconnect();
        }
    }

    private static long _traceState = 0xFFFFFFFF;
    private static readonly PassiveTimer CheckState = new PassiveTimer(0);

    public static long State
    {
        get
        {
            UpdateState();
            return _traceState;
        }
    }

    public static bool IsOn(Channel channel)
    {
        return ((State & (long)channel) != 0);
    }

    private static void UpdateState()
    {
        if (!CheckState.Timeout)
            return;

        if (!Connect() || _tron == null)
            return;

        try
        {
            _tron.WriteByte((byte)TronCommand.CheckTrace);

            var rxLength = 8;
            var rxBuffer = new byte[8];
            rxLength = _tron.Read(rxBuffer, 0, rxLength);

            if (rxLength >= 4)
            {
                _traceState = rxBuffer[0];
                _traceState += (long)rxBuffer[1] << 8;
                _traceState += (long)rxBuffer[2] << 16;
                _traceState += (long)rxBuffer[3] << 24;
            }
        }
        catch (Exception)
        {
            Disconnect();
        }

        CheckState.Start(10000);
    }

    public static void Clear()
    {
        Send(TronCommand.Cls, null);
    }

    // ReSharper disable once EventNeverSubscribedTo.Global
#pragma warning disable MA0046
    public static event Action<string>? OnPrint;
#pragma warning restore MA0046

    public static void Print(string text)
    {
        var data = Encoding.GetEncoding(1252).GetBytes(text + '\0');
        Send(TronCommand.Print, data);

        OnPrint?.Invoke(text);
    }

    public static void PrintLine(string text)
    {
        Print(text + Environment.NewLine);
    }

    public static void PrintCallTrail()
    {
        var stack = new StackTrace();
        var frames = stack.GetFrames();
        if (!frames.Any()) return;

        SetColor(TraceColor.DarkGreen);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        var breadcrumbs = frames.Skip(2)
            .Take(3)
            .Where(f => f.GetMethod() != null)
            .Select(f => f.GetMethod()!.Name)
            .Reverse();
#pragma warning restore CS8602 // Dereference of a possibly null reference.

        PrintLine("CallTrail: " + string.Join(" / ", breadcrumbs));
    }


    private static Color _currentColor = TraceColor.Text;

    public static Color GetColor()
    {
        return _currentColor;
    }
    /// <summary>
    /// Set the output color.
    /// </summary>
    /// <param name="color">Use TraceColor values.</param>
    public static void SetColor(Color color)
    {
        var colorRef = new byte[4];
        colorRef[0] = color.R;
        colorRef[1] = color.G;
        colorRef[2] = color.B;
        colorRef[3] = (color.A == 0xFF) ? (byte)0 : color.A;
        Send(TronCommand.SetColor, colorRef);
        _currentColor = color;
    }

    public static void PrintRxByte(byte data)
    {
        Send(TronCommand.PrintRxByte, new[] { data });
    }
    public static void PrintTxByte(byte data)
    {
        Send(TronCommand.PrintTxByte, new[] { data });
    }

    public static void PrintRxBuffer(byte[] data)
    {
        var len = (uint)data.Length;
        var dataBuffer = new byte[len + 4];
        dataBuffer[0] = (byte)(len % 0xFF);
        dataBuffer[1] = (byte)((len >> 8) % 0xFF);
        dataBuffer[2] = (byte)((len >> 16) % 0xFF);
        dataBuffer[3] = (byte)((len >> 24) % 0xFF);
        data.CopyTo(dataBuffer, 4);
        Send(TronCommand.PrintRxBuffer, dataBuffer);
    }
    public static void PrintTxBuffer(byte[] data)
    {
        var len = (uint)data.Length;
        var dataBuffer = new byte[len + 4];
        dataBuffer[0] = (byte)(len % 0xFF);
        dataBuffer[1] = (byte)((len >> 8) % 0xFF);
        dataBuffer[2] = (byte)((len >> 16) % 0xFF);
        dataBuffer[3] = (byte)((len >> 24) % 0xFF);
        data.CopyTo(dataBuffer, 4);
        Send(TronCommand.PrintTxBuffer, dataBuffer);
    }

    public static void TraceInformation(string text)
    {
        SetColor(TraceColor.Info);
        PrintLine(text);
    }
    public static void TraceWarning(string text)
    {
        SetColor(TraceColor.Warning);
        PrintLine(text);
    }
    public static void TraceError(string text)
    {
        SetColor(TraceColor.Error);
        PrintLine(text);
    }


}