using System.Net;
using System.Net.Sockets;

namespace IctBaden.Framework.Network;

public static class NetworkInfo
{
    public static int GetFreeLocalTcpPort()
    {
#pragma warning disable IDISP001
        var tcpListener = new TcpListener(IPAddress.Loopback, 0);
#pragma warning restore IDISP001
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