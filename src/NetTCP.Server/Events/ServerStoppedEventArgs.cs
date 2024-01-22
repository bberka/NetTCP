namespace NetTCP.Server.Events;

public class ServerStoppedEventArgs
{
  internal ServerStoppedEventArgs(NetTcpServer server) {
    Server = server;
  }

  public NetTcpServer Server { get; }
}