using NetTCP.Network;

namespace NetTCP.Server.Events;

public class MessageHandlerNotFoundEventArgs
{
  public MessageHandlerNotFoundEventArgs(NetTcpConnection connection, ProcessedIncomingPacket packet) {
    Connection = connection;
    Packet = packet;
  }

  public NetTcpConnection Connection { get; }
  public ProcessedIncomingPacket Packet { get; }
}