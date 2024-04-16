namespace NetTCP.Client.Events;

public sealed class ClientConnectedEventArgs
{
  public NetTcpClient Client { get; }

  internal ClientConnectedEventArgs(NetTcpClient client) {
    Client = client;
  }

}