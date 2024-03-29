﻿namespace IctBaden.Framework.Network
{
    using System.Net;
    using System.Net.Sockets;

    public static class NetworkInfo
    {
        public static int GetFreeLocalTcpPort()
        {
            var tcpListener = new TcpListener(IPAddress.Loopback, 0);
            tcpListener.Start();
            var port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
            tcpListener.Stop();
            return port;
        }

        public static int GetFreeLocalUdpPort()
        {
            using var udpClient = new UdpClient(0);
            var port = (udpClient.Client.LocalEndPoint as IPEndPoint)?.Port ?? 0; 
            udpClient.Close();
            return port;
        }

    }
}