namespace NetTCP.Client.Events;

public class ClientConnectedEventArgs
{
  public NetTcpClient Client { get; }

  internal ClientConnectedEventArgs(NetTcpClient client) {
    Client = client;
  }


  public static ClientConnectedEventArgs Create(NetTcpClient client) {
    return new ClientConnectedEventArgs(client);
  }
}