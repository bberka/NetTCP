using NetTCP.Abstract;

namespace NetTCP.Server.Events;

public sealed class PacketQueuedEventArgs
{

  public PacketQueuedEventArgs(NetTcpConnection connection, int messageId, bool encrypted, IPacket message, byte[] bytes) {
    Connection = connection;
    MessageId = messageId;
    Encrypted = encrypted;
    Message = message;
    Bytes = bytes;
  }

  public byte[] Bytes { get; set; }
  public IPacket Message { get; set; }
  public NetTcpConnection Connection { get; }
  public int MessageId { get; }
  public bool Encrypted { get; }
}