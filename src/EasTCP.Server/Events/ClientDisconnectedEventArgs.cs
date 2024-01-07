namespace EasTCP.Server.Events;

public class ClientDisconnectedEventArgs
{
  public ClientDisconnectedEventArgs(EasTcpConnection connection) {
    Connection = connection;
  }

  public EasTcpConnection Connection { get; }
  
  public static ClientDisconnectedEventArgs Create(EasTcpConnection connection) {
    return new(connection);
  }
  
   
}