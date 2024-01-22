namespace NetTCP.Server.Events;

public class ServerErrorEventArgs
{
  internal ServerErrorEventArgs(NetTcpServer server, Exception exception) {
    Server = server;
    Exception = exception;
  }

  public NetTcpServer Server { get; }
  public Exception Exception { get; }
  
}