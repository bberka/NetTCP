namespace NetTCP.Client.Events;

public sealed class HandlerErrorEventArgs
{
  public NetTcpClient Client { get; }
  public Exception Exception { get; }

  internal HandlerErrorEventArgs(NetTcpClient client, Exception exception) {
    Client = client;
    Exception = exception;
  }
}