using NetTCP.Network;

namespace NetTCP.Server.Events;

public sealed class MessageHandlerNotFoundEventArgs
{
  internal MessageHandlerNotFoundEventArgs(NetTcpConnection connection, ProcessedIncomingPacket packet) {
    Connection = connection;
    Packet = packet;
  }

  public NetTcpConnection Connection { get; }
  public ProcessedIncomingPacket Packet { get; }
}