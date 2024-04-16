namespace NetTCP.Server.Events;

public sealed class PacketReceivedEventArgs
{
  internal PacketReceivedEventArgs(NetTcpConnection connection, int messageId, bool encrypted) {
    Connection = connection;
    MessageId = messageId;
    Encrypted = encrypted;
  }

  public NetTcpConnection Connection { get; }
  public int MessageId { get; }
  public bool Encrypted { get; }
}