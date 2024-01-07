namespace EasTCP.Server.Events;

public class ClientConnectedEventArgs
{
  public EasTcpConnection Connection { get;  }

  public ClientConnectedEventArgs(EasTcpConnection connection) {
    Connection = connection;
  }

  public static ClientConnectedEventArgs Create(EasTcpConnection connection) {
    return new(connection);
  }
}