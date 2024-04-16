namespace NetTCP.Client.Events;

public sealed class ClientDisconnectedEventArgs
{
  public NetTcpClient Client { get; }
  public Reason Reason { get; }

  internal ClientDisconnectedEventArgs(NetTcpClient client,Reason reason) {
    Client = client;
    Reason = reason;
  }

}