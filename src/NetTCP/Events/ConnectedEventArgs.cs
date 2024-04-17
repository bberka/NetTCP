using NetTCP.Abstract;

namespace NetTCP.Events;

public readonly struct ConnectedEventArgs
{
  public INetTcpSession Session { get; }

  public ConnectedEventArgs(INetTcpSession session) {
    Session = session;
  }
}