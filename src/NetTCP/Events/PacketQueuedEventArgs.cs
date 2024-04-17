using NetTCP.Abstract;

namespace NetTCP.Events;

public readonly struct PacketQueuedEventArgs
{
  public PacketQueuedEventArgs(INetTcpSession session, int opCode, bool encrypted, PacketQueueType packetQueueType) {
    Session = session;
    OpCode = opCode;
    Encrypted = encrypted;
    PacketQueueType = packetQueueType;
  }

  public INetTcpSession Session { get; }
  public int OpCode { get; }
  public bool Encrypted { get; }
  public PacketQueueType PacketQueueType { get; }
}