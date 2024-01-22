using NetTCP.Abstract;

namespace NetTCP.Server.Events;

public class UnknownPacketSendAttemptEventArgs
{
  internal UnknownPacketSendAttemptEventArgs(NetTcpConnection connection, IPacket message, bool encrypted) {
    Connection = connection;
    Message = message;
    Encrypted = encrypted;
  }

  public NetTcpConnection Connection { get; }
  public IPacket Message { get; }
  public bool Encrypted { get; }
}