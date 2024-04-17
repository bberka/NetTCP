namespace NetTCP.Server.Events;

public readonly struct ServerErrorEventArgs
{
  public ServerErrorEventArgs(Exception exception) {
    Exception = exception;
  }
  public Exception Exception { get; }
  
}