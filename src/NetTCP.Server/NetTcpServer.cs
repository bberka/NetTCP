using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using Autofac;
using NetTCP.Abstract;
using NetTCP.Server.Events;

namespace NetTCP.Server;

public class NetTcpServer
{
  protected readonly NetServerPacketContainer PacketContainer;
  protected IPAddress ListenIpAddress { get; }
  protected ushort Port { get; }
  protected TcpListener Listener { get; }
  
  /// <summary>
  /// Connection timeout in seconds
  /// </summary>
  public int ConnectionTimeoutSeconds { get; protected set; } = 60 * 5; // 5 minutes
  protected CancellationTokenSource ServerCancellationTokenSource { get; }
  public ConcurrentBag<NetTcpConnection> Connections { get; }

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

  internal NetTcpServer(IPAddress ipAddress, ushort port, NetServerPacketContainer packetContainer) {
    PacketContainer = packetContainer;
    ListenIpAddress = ipAddress;
    Port = port;
    //TODO SET SOCKET OPTION
    Listener = new TcpListener(ListenIpAddress, port);
    ServerCancellationTokenSource = new CancellationTokenSource();
    Connections = new ConcurrentBag<NetTcpConnection>();
    _ = Task.Run(HandleConnectionTimeouts,
                 ServerCancellationTokenSource.Token);
  }

  private async Task HandleConnectionTimeouts() {
    const int timeoutTaskDelay = 10;
    while (ServerCancellationTokenSource.IsCancellationRequested == false) {
      await Task.Delay(TimeSpan.FromSeconds(timeoutTaskDelay)).ConfigureAwait(false);
      var tempConnections = new List<NetTcpConnection>(Connections.Count);
      while (Connections.TryTake(out var connection)) tempConnections.Add(connection);
      foreach (var tcpConnection in tempConnections) {
        if (tcpConnection.LastActivity + ConnectionTimeoutSeconds * 1000 < Environment.TickCount64)
          tcpConnection.DisconnectByServer();
        else
          Connections.Add(tcpConnection);
      }
    }
  }


  /// <summary>
  /// Starts listening and handling for incoming connections
  /// </summary>
  /// <param name="blockThread">Whether the listener will block created thread</param>
  public async Task StartServerAsync() {
    try {
      Listener.Start();
      ServerStarted?.Invoke(this, new ServerStartedEventArgs(this));
      await Task.Run(async () => {
                       while (ServerCancellationTokenSource.IsCancellationRequested == false) {
                         var client = await Listener.AcceptTcpClientAsync().ConfigureAwait(false);
                         var connection = new NetTcpConnection(client, PacketContainer, ServerCancellationTokenSource.Token);
                         connection.SubscribeToEvents(this);
                         ClientConnected?.Invoke(this, new ClientConnectedEventArgs(connection));
                         Connections.Add(connection);
                       }
                     },
                     ServerCancellationTokenSource.Token);
    }
    catch (Exception ex) {
      ServerError?.Invoke(this, new ServerErrorEventArgs(this, ex));
      throw;
    }
  }

  public void StopServer() {
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