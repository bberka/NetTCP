using NetTCP.Abstract;

namespace NetTCP.Events;

public readonly struct UnknownPacketReceivedEventArgs
{
  public UnknownPacketReceivedEventArgs(INetTcpSession session, int messageId, bool encrypted, int size, byte[] packet) {
    Session = session;
    MessageId = messageId;
    Encrypted = encrypted;
    Size = size;
    Packet = packet;
  }

  public INetTcpSession Session { get; }
  public int MessageId { get; }
  public bool Encrypted { get; }
  public int Size { get; }
  public byte[] Packet { get; }
}