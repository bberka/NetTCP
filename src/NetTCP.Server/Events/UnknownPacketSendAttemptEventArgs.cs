
using NetTCP.Abstract;

namespace NetTCP.Server.Events;

public class UnknownPacketSendAttemptEventArgs
{
  public UnknownPacketSendAttemptEventArgs(EasTcpConnection connection, IPacketWriteable message, bool encrypted) {
    Connection = connection;
    Message = message;
    Encrypted = encrypted;
  }
  public EasTcpConnection Connection { get;  }
  public IPacketWriteable Message { get; }
  public bool Encrypted { get; }
}