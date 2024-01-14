namespace NetTCP.Server.Events;

public class ConnectionErrorEventArgs
{
  public ConnectionErrorEventArgs(EasTcpConnection connection, Exception exception) {
    Connection = connection;
    Exception = exception;
  }

  public EasTcpConnection Connection { get; }
  public Exception Exception { get; }
  
  public static ConnectionErrorEventArgs Create(EasTcpConnection connection, Exception exception) {
    return new(connection, exception);
  }
}