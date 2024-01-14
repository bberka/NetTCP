using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using NetTCP.Abstract;
using NetTCP.Server.Events;

namespace NetTCP.Server;

public class EasTcpServer
{
  protected IPAddress ListenIpAddress { get; }
  protected ushort Port { get; }
  protected TcpListener Listener { get; }
  public int ConnectionTimeout { get; protected set; }
  protected CancellationTokenSource ServerCancellationTokenSource { get; }
  public ConcurrentBag<EasTcpConnection> Connections { get; }

  public bool CanProcess => Listener.Server.IsBound && !ServerCancellationTokenSource.IsCancellationRequested;

  public event EventHandler<ClientConnectedEventArgs> ClientConnected;
  public event EventHandler<ServerStartedEventArgs> ServerStarted;
  public event EventHandler<ServerStoppedEventArgs> ServerStopped;
  public event EventHandler<ServerErrorEventArgs> ServerError;
  public event EventHandler<ConnectionErrorEventArgs> ConnectionError;
  public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;
  public event EventHandler<UnknownPacketReceivedEventArgs> UnknownPacketReceived;
  public event EventHandler<UnknownPacketSendAttemptEventArgs> UnknownPacketSendAttempted;
  public event EventHandler<MessageHandlerNotFoundEventArgs> MessageHandlerNotFound;
  public event EventHandler<PacketQueuedEventArgs> PacketQueued;
  public event EventHandler<PacketReceivedEventArgs> PacketReceived;
  public EasTcpServer(string listenAddress, ushort port) {
    ListenIpAddress = IPAddress.Parse(listenAddress);
    Port = port;
    //TODO SET SOCKET OPTION
    Listener = new TcpListener(ListenIpAddress, port);
    ServerCancellationTokenSource = new CancellationTokenSource();
    Connections = new ConcurrentBag<EasTcpConnection>();
    _ = Task.Run(HandleConnectionTimeouts,
                 ServerCancellationTokenSource.Token);
  }

  private async Task HandleConnectionTimeouts() {
    const int timeoutTaskDelay = 10;
    while (true) {
      await Task.Delay(TimeSpan.FromSeconds(timeoutTaskDelay)).ConfigureAwait(false);
      var tempConnections = new List<EasTcpConnection>(Connections.Count);
      while (Connections.TryTake(out var connection)) tempConnections.Add(connection);
      foreach (var tcpConnection in tempConnections) {
        if (tcpConnection.LastActivity + ConnectionTimeout < Environment.TickCount64)
          tcpConnection.DisconnectByServer();
        else
          Connections.Add(tcpConnection);
      }
    }
  }

  private void StartAcceptConnections(bool blockThread) {
    var connectionHandlerTask = Task.Run(async () => {
                                           while (true) {
                                             var client = await Listener.AcceptTcpClientAsync().ConfigureAwait(false);
                                             var connection = new EasTcpConnection(client, ServerCancellationTokenSource.Token);
                                             connection.SubscribeToEvents(this);
                                             ClientConnected?.Invoke(this, new ClientConnectedEventArgs(connection));
                                             Connections.Add(connection);
                                           }
                                         },
                                         ServerCancellationTokenSource.Token);
    if (blockThread) connectionHandlerTask.Wait();
  }

  /// <summary>
  /// Starts listening and handling for incoming connections
  /// </summary>
  /// <param name="blockThread">Whether the listener will block created thread</param>
  public void StartServer(bool blockThread = false) {
    try {
      Listener.Start();
      ServerStarted?.Invoke(this, new ServerStartedEventArgs(this));
      StartAcceptConnections(blockThread);
    }
    catch (Exception ex) {
      ServerError?.Invoke(this, new ServerErrorEventArgs(this, ex));
      throw;
    }
  }

  public void StopServer() {
    //Stop listening new connections
    Listener.Stop();
    ServerStopped?.Invoke(this, new ServerStoppedEventArgs(this));
    //TODO Wait all handlers to finish
    foreach (var connection in Connections) {
      connection.DisconnectByServer(DisconnectReason.ServerStopped);
    }

    ServerCancellationTokenSource.Cancel();
    ServerCancellationTokenSource.Dispose();
    Listener.Server.Dispose();
  }

  public void EnqueueBroadcastPacket(IPacketWriteable message,
                                     bool encrypted = false) {
    foreach (var connection in Connections) {
      connection.EnqueuePacketSend(message, encrypted);
    }
  }
}