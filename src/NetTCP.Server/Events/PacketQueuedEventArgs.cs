using NetTCP.Abstract;

namespace NetTCP.Server.Events;

public sealed class PacketQueuedEventArgs
{
  public PacketQueuedEventArgs(NetTcpConnection connection, int opCode, bool encrypted, PacketQueueType packetQueueType) {
    Connection = connection;
    OpCode = opCode;
    Encrypted = encrypted;
    PacketQueueType = packetQueueType;
  }
  public NetTcpConnection Connection { get; }
  public int OpCode { get; }
  public bool Encrypted { get; }
  public PacketQueueType PacketQueueType { get; }
}