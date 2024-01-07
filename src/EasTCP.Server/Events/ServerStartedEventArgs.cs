namespace EasTCP.Server.Events;

public class ServerStartedEventArgs
{
  public ServerStartedEventArgs(EasTcpServer server) {
    Server = server;
  }

  public EasTcpServer Server { get; }
  
  
}