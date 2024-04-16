using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Autofac;
using NetTCP.Abstract;
using NetTCP.Server.Events;

namespace NetTCP.Server;

public class NetTcpServer
{
  protected internal NetTcpPacketManager<NetTcpConnection> PacketManager { get; }
  protected IPAddress ListenIpAddress { get; }
  public ushort Port { get; }
  public string IpAddress => ListenIpAddress.ToString();
  protected TcpListener Listener { get; }

  /// <summary>
  ///   Connection timeout in seconds
  /// </summary>
  public int ConnectionTimeoutSeconds { get; protected set; } = 30; // 30 seconds

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
  public event EventHandler<HandlerErrorEventArgs> HandlerError;

  public IContainer ServiceContainer { get; private set; }

  public NetTcpServer(string ip, ushort port, IContainer serviceContainer, Assembly[]? packetAssemblies = null) {
    if (string.IsNullOrEmpty(ip)) {
      throw new ArgumentException("Empty ip address: " + ip, nameof(ip));
    }

    var parseIp = IPAddress.TryParse(ip, out var ipAddress);
    if (parseIp == false)
      throw new ArgumentException("Invalid ip address: " + ip, nameof(ip));
    var isValidPort = NetTcpTools.IsValidPort(port);
    if (isValidPort == false)
      throw new ArgumentException("Invalid port: " + port, nameof(port));

    if (serviceContainer == null) {
      throw new ArgumentNullException(nameof(serviceContainer));
    }

    PacketManager = new NetTcpPacketManager<NetTcpConnection>();
    packetAssemblies ??= Array.Empty<Assembly>();
    foreach (var assembly in packetAssemblies) {
      Debug.WriteLine("Registering assembly: " + assembly.FullName, "NetTcpServer");
      PacketManager.Register(assembly);
    }

    PacketManager.Initialize();
    ServiceContainer = serviceContainer;
    ListenIpAddress = ipAddress;
    Port = port;
    Listener = new TcpListener(ListenIpAddress, port);
    ServerCancellationTokenSource = new CancellationTokenSource();
    Connections = new ConcurrentBag<NetTcpConnection>();
    _ = Task.Run(HandleConnectionTimeouts,
                 ServerCancellationTokenSource.Token);
  }


  private async Task HandleConnectionTimeouts() {
    const int timeoutTaskDelay = 10;
    while (CanProcess && ServerCancellationTokenSource?.IsCancellationRequested == false) {
      await Task.Delay(TimeSpan.FromSeconds(timeoutTaskDelay)).ConfigureAwait(false);
      var tempConnections = new List<NetTcpConnection>(Connections.Count);
      while (Connections.TryTake(out var connection)) tempConnections.Add(connection);
      foreach (var tcpConnection in tempConnections) {
        if (tcpConnection.LastActivity + ConnectionTimeoutSeconds * 1000 < Environment.TickCount64) {
          Debug.WriteLine("Connection timeout: " + tcpConnection.RemoteIpAddress, "NetTcpServer");
          tcpConnection.Disconnect(Reason.Timeout);
          continue;
        }

        if (!tcpConnection.CanRead) {
          Debug.WriteLine("Connection can not process: " + tcpConnection.RemoteIpAddress, "NetTcpServer");
          tcpConnection.Disconnect(Reason.CanNotProcess);
          continue;
        }

        Connections.Add(tcpConnection);
      }
    }
  }


  /// <summary>
  ///   Starts listening and handling for incoming connections
  /// </summary>
  public async Task StartServerAsync() {
    try {
      Listener.Start();
      ServerStarted?.Invoke(this, new ServerStartedEventArgs());
      Debug.WriteLine("Server started on " + IpAddress + ":" + Port, "NetTcpServer");
    }
    catch (Exception ex) {
      ServerError?.Invoke(this, new ServerErrorEventArgs(ex));
      Debug.WriteLine("Error starting server: " + ex.Message, "NetTcpServer");
      throw;
    }
    await Task.Run(async () => {
                     while (ServerCancellationTokenSource?.IsCancellationRequested == false) {
                       try {
                         Debug.WriteLine("Waiting for new connection", "NetTcpServer");
                         var client = await Listener.AcceptTcpClientAsync(ServerCancellationTokenSource.Token).ConfigureAwait(false);
                         var connection = new NetTcpConnection(client, this, ServerCancellationTokenSource.Token);
                         Debug.WriteLine("New connection accepted: " + connection.RemoteIpAddress, "NetTcpServer");
                         Connections.Add(connection);
                         ClientConnected?.Invoke(this, new ClientConnectedEventArgs(connection));
                       }
                       catch (Exception ex) {
                         Debug.WriteLine("Error accepting new connection: " + ex.Message, "NetTcpServer");
                         ServerError?.Invoke(this, new ServerErrorEventArgs(ex));
                       }
                     }
                   },
                   ServerCancellationTokenSource.Token);
  }

  public void StopServer(Reason reason) {
    Debug.WriteLine("Stopping server", "NetTcpServer");
    Listener.Stop();
    var disconnectTasks = new List<Task>();
    foreach (var connection in Connections) {
      disconnectTasks.Add(Task.Run(() => connection.Disconnect(reason)));
    }
    Task.WhenAll(disconnectTasks).GetAwaiter().GetResult();
    ServerCancellationTokenSource.Dispose();
    Listener.Server.Dispose();
    ServerStopped?.Invoke(this, new ServerStoppedEventArgs());
    Debug.WriteLine("Server stopped", "NetTcpServer");
  }

  public void EnqueueBroadcastPacket(IPacket message,
                                     bool encrypted = false) {
    Debug.WriteLine("Broadcasting packet: " + message, "NetTcpServer");
    Parallel.ForEach(Connections,
                     connection => connection.EnqueuePacketSend(message, encrypted));
    // foreach (var connection in Connections)
    //   connection.EnqueuePacketSend(message, encrypted);
  }

  internal void InvokeClientDisconnected(ClientDisconnectedEventArgs args) {
    ClientDisconnected?.Invoke(this, args);
  }

  internal void InvokeConnectionError(ConnectionErrorEventArgs args) {
    ConnectionError?.Invoke(this, args);
  }

  internal void InvokeUnknownPacketReceived(UnknownPacketReceivedEventArgs args) {
    UnknownPacketReceived?.Invoke(this, args);
  }

  internal void InvokeUnknownPacketSendAttempt(UnknownPacketSendAttemptEventArgs args) {
    UnknownPacketSendAttempted?.Invoke(this, args);
  }

  internal void InvokeMessageHandlerNotFound(MessageHandlerNotFoundEventArgs args) {
    MessageHandlerNotFound?.Invoke(this, args);
  }

  internal void InvokePacketQueued(PacketQueuedEventArgs args) {
    PacketQueued?.Invoke(this, args);
  }

  internal void InvokePacketReceived(PacketReceivedEventArgs args) {
    PacketReceived?.Invoke(this, args);
  }

  internal void InvokeHandlerError(HandlerErrorEventArgs args) {
    HandlerError?.Invoke(this, args);
  }
}