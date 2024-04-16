using NetTCP.Network;

namespace NetTCP.Client.Events;

public sealed class MessageHandlerNotFoundEventArgs
{
  internal MessageHandlerNotFoundEventArgs(NetTcpClient client, ProcessedIncomingPacket processedIncomingPacket) {
    Client = client;
    ProcessedIncomingPacket = processedIncomingPacket;
  }

  public NetTcpClient Client { get; }
  public ProcessedIncomingPacket ProcessedIncomingPacket { get; }
}