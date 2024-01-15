
using NetTCP.Abstract;

namespace NetTCP.Server.Events;

public class UnknownPacketSendAttemptEventArgs
{
  public UnknownPacketSendAttemptEventArgs(NetTcpConnection connection, IWriteablePacket message, bool encrypted) {
    Connection = connection;
    Message = message;
    Encrypted = encrypted;
  }
  public NetTcpConnection Connection { get;  }
  public IWriteablePacket Message { get; }
  public bool Encrypted { get; }
}