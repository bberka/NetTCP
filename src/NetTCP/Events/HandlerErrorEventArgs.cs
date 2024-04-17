using NetTCP.Abstract;

namespace NetTCP.Events;

public readonly struct HandlerErrorEventArgs
{ 
  public INetTcpSession Session { get; }
  public Exception Exception { get; }

  public HandlerErrorEventArgs(INetTcpSession session, Exception exception) {
    Session = session;
    Exception = exception;
  }
}