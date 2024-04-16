namespace NetTCP.Server.Events;

public sealed class ServerErrorEventArgs
{
  internal ServerErrorEventArgs(Exception exception) {
    Exception = exception;
  }
  public Exception Exception { get; }
  
}