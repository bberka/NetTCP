namespace NetTCP.Server.Events;

public class ServerStartedEventArgs
{
  public ServerStartedEventArgs(NetTcpServer server) {
    Server = server;
  }

  public NetTcpServer Server { get; }
  
  
}