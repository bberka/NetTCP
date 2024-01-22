namespace NetTCP.Server.Events;

public class ClientConnectedEventArgs
{
  internal ClientConnectedEventArgs(NetTcpConnection connection) {
    Connection = connection;
  }
  public NetTcpConnection Connection { get; }
}