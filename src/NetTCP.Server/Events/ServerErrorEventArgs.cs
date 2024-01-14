namespace NetTCP.Server.Events;

public class ServerErrorEventArgs
{
  public ServerErrorEventArgs(EasTcpServer server, Exception exception) {
    Server = server;
    Exception = exception;
  }

  public EasTcpServer Server { get; }
  public Exception Exception { get; }
  
  public static ServerErrorEventArgs Create(EasTcpServer server, Exception exception) {
    return new(server, exception);
  }
}