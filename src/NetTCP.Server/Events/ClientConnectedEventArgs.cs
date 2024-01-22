namespace NetTCP.Server.Events;

public class ClientConnectedEventArgs
{
  public ClientConnectedEventArgs(NetTcpConnection connection) {
    Connection = connection;
  }

  public NetTcpConnection Connection { get; }

  public static ClientConnectedEventArgs Create(NetTcpConnection connection) {
    return new ClientConnectedEventArgs(connection);
  }
}