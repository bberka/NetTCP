namespace NetTCP.Server.Events;

public sealed class ConnectionErrorEventArgs
{
  internal ConnectionErrorEventArgs(NetTcpConnection connection, Exception exception, Reason reason) {
    Connection = connection;
    Exception = exception;
    Reason = reason;
  }

  public NetTcpConnection Connection { get; }
  public Exception Exception { get; }
  public Reason Reason { get; }
}