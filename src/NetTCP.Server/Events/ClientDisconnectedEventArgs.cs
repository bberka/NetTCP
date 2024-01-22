namespace NetTCP.Server.Events;

public class ClientDisconnectedEventArgs
{
  internal ClientDisconnectedEventArgs(NetTcpConnection connection, Reason reason) {
    Connection = connection;
    Reason = reason;
  }

  public NetTcpConnection Connection { get; }
  public Reason Reason { get; }
}