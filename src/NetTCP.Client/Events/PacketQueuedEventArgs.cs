namespace NetTCP.Client.Events;

public sealed class PacketQueuedEventArgs
{
  internal PacketQueuedEventArgs(NetTcpClient client, int opCode, bool encrypted, PacketQueueType packetQueueType) {
    Client = client;
    OpCode = opCode;
    Encrypted = encrypted;
    PacketQueueType = packetQueueType;
  }

  public NetTcpClient Client { get; }
  public int OpCode { get; }
  public bool Encrypted { get; }
  public PacketQueueType PacketQueueType { get; }
}