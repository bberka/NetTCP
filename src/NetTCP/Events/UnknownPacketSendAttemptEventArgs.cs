using NetTCP.Abstract;

namespace NetTCP.Events;

public readonly struct UnknownPacketSendAttemptEventArgs
{
  public UnknownPacketSendAttemptEventArgs(INetTcpSession session, IPacket message, bool encrypted) {
    Session = session;
    Message = message;
    Encrypted = encrypted;
  }

  public INetTcpSession Session { get; }
  public IPacket Message { get; }
  public bool Encrypted { get; }
}