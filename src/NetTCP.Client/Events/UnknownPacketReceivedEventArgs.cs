namespace NetTCP.Client.Events;

public sealed class UnknownPacketReceivedEventArgs
{
  internal UnknownPacketReceivedEventArgs(NetTcpClient client, int messageId, bool encrypted, int size, byte[] packet) {
    Client = client;
    MessageId = messageId;
    Encrypted = encrypted;
    Size = size;
    Packet = packet;
  }

  public NetTcpClient Client { get; }
  public int MessageId { get; }
  public bool Encrypted { get; }
  public int Size { get; }
  public byte[] Packet { get; }
}