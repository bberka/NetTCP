using NetTCP.Abstract;

namespace NetTCP.Events;

public readonly struct PacketReceivedEventArgs
{
  public PacketReceivedEventArgs(INetTcpSession session, int messageId, bool encrypted) {
    Session = session;
    MessageId = messageId;
    Encrypted = encrypted;
  }

  public INetTcpSession Session { get; }
  public int MessageId { get; }
  public bool Encrypted { get; }
}