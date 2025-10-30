using System.Net.Sockets;

public class ConnectedClient
{
    public string Id { get; set; }
    public string IpAddress { get; set; }
    public TcpClient ClientSocket { get; set; }
}
