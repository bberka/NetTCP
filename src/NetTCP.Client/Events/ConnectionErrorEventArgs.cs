namespace NetTCP.Client.Events;

public class ConnectionErrorEventArgs
{
  internal ConnectionErrorEventArgs(NetTcpClient client, Exception exception, Reason reason) {
    Client = client;
    Exception = exception;
    Reason = reason;
  }

  public NetTcpClient Client { get; }
  public Exception Exception { get; }
  public Reason Reason { get; }
}