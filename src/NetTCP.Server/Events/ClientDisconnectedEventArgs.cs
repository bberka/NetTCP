namespace NetTCP.Server.Events;

public class ClientDisconnectedEventArgs
{
  public ClientDisconnectedEventArgs(NetTcpConnection connection) {
    Connection = connection;
  }

  public NetTcpConnection Connection { get; }
  
  public static ClientDisconnectedEventArgs Create(NetTcpConnection connection) {
    return new(connection);
  }
  
   
}