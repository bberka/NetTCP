namespace NetTCP.Client.Events;

public class PacketReceivedEventArgs
{
  internal PacketReceivedEventArgs(NetTcpClient client, int messageId, bool encrypted) {
    Client = client;
    MessageId = messageId;
    Encrypted = encrypted;
  }

  public NetTcpClient Client { get; }
  public int MessageId { get; }
  public bool Encrypted { get; }
}