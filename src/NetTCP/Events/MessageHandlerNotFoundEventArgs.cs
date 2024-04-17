using NetTCP.Abstract;
using NetTCP.Network;

namespace NetTCP.Events;

public readonly struct MessageHandlerNotFoundEventArgs
{
  public INetTcpSession Session { get; }
  public ProcessedIncomingPacket ProcessedIncomingPacket { get; }

  public MessageHandlerNotFoundEventArgs(INetTcpSession session, ProcessedIncomingPacket processedIncomingPacket) {
    Session = session;
    ProcessedIncomingPacket = processedIncomingPacket;
  }
}