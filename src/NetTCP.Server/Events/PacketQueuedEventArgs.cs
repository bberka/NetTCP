namespace NetTCP.Server.Events;

public class PacketQueuedEventArgs
{
  public PacketQueuedEventArgs(EasTcpConnection connection, int messageId, bool encrypted) {
    Connection = connection;
    MessageId = messageId;
    Encrypted = encrypted;
  }

  public EasTcpConnection Connection { get; }
  public int MessageId { get; }
  public bool Encrypted { get; }
}