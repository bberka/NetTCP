using NetTCP.Abstract;

namespace NetTCP.Events;

public readonly struct DisconnectedEventArgs
{
  public INetTcpSession Session { get; }
  public NetTcpErrorReason NetTcpErrorReason { get; }

  public DisconnectedEventArgs(INetTcpSession session, NetTcpErrorReason netTcpErrorReason) {
    NetTcpErrorReason = netTcpErrorReason;
    Session = session;
  }
}