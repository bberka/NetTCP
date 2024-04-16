namespace NetTCP.Client.Events;

public sealed class PacketQueuedEventArgs
{
  internal PacketQueuedEventArgs(NetTcpClient client, int messageId, bool encrypted, int size) {
    Client = client;
    MessageId = messageId;
    Encrypted = encrypted;
  }

  public NetTcpClient Client { get; }
  public int MessageId { get; }
  public bool Encrypted { get; }
}