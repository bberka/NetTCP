namespace NetTCP.Server.Events;

public class UnknownPacketReceivedEventArgs
{
  public UnknownPacketReceivedEventArgs(EasTcpConnection connection, int messageId, bool encrypted, int size,byte[] packet) {
    Connection = connection;
    MessageId = messageId;
    Encrypted = encrypted;
    Size = size;
    Packet = packet;
  }

  public EasTcpConnection Connection { get; }
  public int MessageId { get; }
  public bool Encrypted { get; }
  public int Size { get; }
  public byte[] Packet { get; }

}