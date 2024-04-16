namespace NetTCP.Server.Events;

public sealed class ClientConnectedEventArgs
{
  internal ClientConnectedEventArgs(NetTcpConnection connection) {
    Connection = connection;
  }
  public NetTcpConnection Connection { get; }
}