﻿using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using NetTCP.Abstract;
using NetTCP.Server.Events;

namespace NetTCP.Server;

public class NetTcpServer
{
  protected internal ISerializer Serializer { get; }
  protected internal NetTcpServerPacketContainer PacketContainer { get; }


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

  internal NetTcpServer(IPAddress ipAddress, ushort port, NetTcpServerPacketContainer packetContainer, ISerializer serializer) {
    PacketContainer = packetContainer;
    Serializer = serializer;
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
    while (ServerCancellationTokenSource?.IsCancellationRequested == false) {
      await Task.Delay(TimeSpan.FromSeconds(timeoutTaskDelay)).ConfigureAwait(false);
      var tempConnections = new List<NetTcpConnection>(Connections.Count);
      while (Connections.TryTake(out var connection)) tempConnections.Add(connection);
      foreach (var tcpConnection in tempConnections) {
        if (tcpConnection.LastActivity + ConnectionTimeoutSeconds * 1000 < Environment.TickCount64) {
          tcpConnection.DisconnectByServer(Reason.Timeout);
          continue;
        }

        if (!tcpConnection.CanProcess) {
          tcpConnection.DisconnectByServer(Reason.CanNotProcess);
          continue;
        }

        Connections.Add(tcpConnection);
      }
    }
  }


  /// <summary>
  ///   Starts listening and handling for incoming connections
  /// </summary>
  /// <param name="blockThread">Whether the listener will block created thread</param>
  public async Task StartServerAsync() {
    try {
      Listener.Start();
      ServerStarted?.Invoke(this, new ServerStartedEventArgs(this));
    }
    catch (Exception ex) {
      ServerError?.Invoke(this, new ServerErrorEventArgs(this, ex));
      throw;
    }

    try {
      await Task.Run(async () => {
                       while (ServerCancellationTokenSource?.IsCancellationRequested == false) {
                         var client = await Listener.AcceptTcpClientAsync(ServerCancellationTokenSource.Token).ConfigureAwait(false);
                         var connection = new NetTcpConnection(client, this, ServerCancellationTokenSource.Token);
                         Connections.Add(connection);
                         ClientConnected?.Invoke(this, new ClientConnectedEventArgs(connection));
                       }
                     },
                     ServerCancellationTokenSource.Token);
    }
    catch (Exception ex) {
      //Ignore
    }
  }

  public void StopServer(Reason reason) {
    Listener.Stop();
    ServerStopped?.Invoke(this, new ServerStoppedEventArgs(this));
    //TODO Wait all handlers to finish
    foreach (var connection in Connections)
      connection.DisconnectByServer(Reason.ServerStopped);

    ServerCancellationTokenSource.Cancel();
    ServerCancellationTokenSource.Dispose();
    Listener.Server.Dispose();
  }

  public void EnqueueBroadcastPacket(IPacket message,
                                     bool encrypted = false) {
    foreach (var connection in Connections)
      connection.EnqueuePacketSend(message, encrypted);
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
}