namespace NetTCP.Server.Events;

public sealed class HandlerErrorEventArgs
{
  public NetTcpConnection Connection { get; }
  public Exception Exception { get; }

  internal HandlerErrorEventArgs(NetTcpConnection connection, Exception exception) {
    Connection = connection;
    Exception = exception;
  }
}