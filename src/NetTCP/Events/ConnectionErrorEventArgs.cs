using NetTCP.Abstract;

namespace NetTCP.Events;

public readonly struct ConnectionErrorEventArgs
{
  public INetTcpSession Session { get; }
  
  public Exception Exception { get; }
  
  public NetTcpErrorReason NetTcpErrorReason { get; }

  public ConnectionErrorEventArgs(INetTcpSession session, Exception exception, NetTcpErrorReason netTcpErrorReason) {
    Session = session;
    Exception = exception;
    NetTcpErrorReason = netTcpErrorReason;
  }
  
 
}