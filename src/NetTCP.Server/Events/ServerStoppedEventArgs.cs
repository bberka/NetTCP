namespace NetTCP.Server.Events;

public class ServerStoppedEventArgs
{
  public ServerStoppedEventArgs(EasTcpServer server) {
    Server = server;
  }

  public EasTcpServer Server { get; }
  
   
}