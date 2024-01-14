namespace NetTCP.Server.Events;

public class ServerStoppedEventArgs
{
  public ServerStoppedEventArgs(NetTcpServer server) {
    Server = server;
  }

  public NetTcpServer Server { get; }
  
   
}