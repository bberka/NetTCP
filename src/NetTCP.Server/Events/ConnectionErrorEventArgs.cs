namespace NetTCP.Server.Events;

public class ConnectionErrorEventArgs
{
  public ConnectionErrorEventArgs(NetTcpConnection connection, Exception exception) {
    Connection = connection;
    Exception = exception;
  }

  public NetTcpConnection Connection { get; }
  public Exception Exception { get; }

  public static ConnectionErrorEventArgs Create(NetTcpConnection connection, Exception exception) {
    return new ConnectionErrorEventArgs(connection, exception);
  }
}