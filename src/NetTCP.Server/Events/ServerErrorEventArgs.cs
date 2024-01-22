namespace NetTCP.Server.Events;

public class ServerErrorEventArgs
{
  public ServerErrorEventArgs(NetTcpServer server, Exception exception) {
    Server = server;
    Exception = exception;
  }

  public NetTcpServer Server { get; }
  public Exception Exception { get; }

  public static ServerErrorEventArgs Create(NetTcpServer server, Exception exception) {
    return new ServerErrorEventArgs(server, exception);
  }
}