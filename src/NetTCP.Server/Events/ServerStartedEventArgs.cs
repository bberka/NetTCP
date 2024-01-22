namespace NetTCP.Server.Events;

public class ServerStartedEventArgs
{
  internal ServerStartedEventArgs(NetTcpServer server) {
    Server = server;
  }

  public NetTcpServer Server { get; }
}