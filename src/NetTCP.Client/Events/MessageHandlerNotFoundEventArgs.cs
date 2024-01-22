using NetTCP.Network;

namespace NetTCP.Client.Events;

public class MessageHandlerNotFoundEventArgs
{
  internal MessageHandlerNotFoundEventArgs(NetTcpClient client, ProcessedIncomingPacket packet) {
    Client = client;
    Packet = packet;
  }

  public NetTcpClient Client { get; }
  public ProcessedIncomingPacket Packet { get; }
}