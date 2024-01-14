namespace NetTCP.Server.Events;

public class ClientConnectedEventArgs
{
  public NetTcpConnection Connection { get;  }

  public ClientConnectedEventArgs(NetTcpConnection connection) {
    Connection = connection;
  }

  public static ClientConnectedEventArgs Create(NetTcpConnection connection) {
    return new(connection);
  }
}