using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Autofac;
using NetTCP.Abstract;
using NetTCP.Network;
using NetTCP.Server.Events;

namespace NetTCP.Server;

public sealed class NetTcpConnection : NetTcpConnectionBase
{
  private readonly NetTcpServer _server;

  protected CancellationToken ServerCancellationToken { get; set; }

  public NetTcpConnection(TcpClient client, NetTcpServer server, CancellationToken serverCancellationToken) {
    _server = server;
    Client = client;
    Scope = _server.ServiceContainer.BeginLifetimeScope();
    ServerCancellationToken = serverCancellationToken;
    CancellationTokenSource = new CancellationTokenSource();
    IncomingPacketQueue = new ConcurrentQueue<ProcessedIncomingPacket>();
    OutgoingPacketQueue = new ConcurrentQueue<ProcessedOutgoingPacket>();
    ConnectedAtUtc = DateTime.UtcNow;
    RemoteIpAddress = (string)((IPEndPoint)Client.Client.RemoteEndPoint).Address.ToString();
    RemotePort = (ushort)((IPEndPoint)Client.Client.RemoteEndPoint).Port;
    Task.Run(HandleStream, CancellationTokenSource.Token);
    Task.Run(HandleOutgoingPackets, CancellationTokenSource.Token);
    Task.Run(HandleIncomingPackets);
  }

  protected override void HandleStream() {
    Debug.WriteLine($"Waiting for data from {RemoteIpAddress}");
    while (CanRead && !ServerCancellationToken.IsCancellationRequested) {
      try {
        ServerCancellationToken.ThrowIfCancellationRequested();
        LastActivity = Environment.TickCount64;
        var messageId = BinaryReader.ReadInt32();
        var encrypted = BinaryReader.ReadBoolean();
        var size = BinaryReader.ReadInt32();
        var restBytes = BinaryReader.ReadBytes(size);

        var messageInstance = _server.PacketManager.GetMessage(messageId);
        if (messageInstance == null) {
          _server.InvokeUnknownPacketReceived(new UnknownPacketReceivedEventArgs(this, messageId, encrypted, size, restBytes));
          Debug.WriteLine($"Unknown packet received from {RemoteIpAddress} with message id {messageId}");
          return;
        }

        messageInstance.Read(new TcpPacketReader(restBytes));
        var clientPacket = new ProcessedIncomingPacket(messageId, encrypted, messageInstance);
        IncomingPacketQueue.Enqueue(clientPacket);
        Debug.WriteLine($"Packet received from {RemoteIpAddress} with message id {messageId}");
        _server.InvokePacketQueued(new PacketQueuedEventArgs(this, messageId, encrypted, PacketQueueType.Incoming));
      }
      catch (Exception ex) {
        _server.InvokeConnectionError(new ConnectionErrorEventArgs(this, ex, Reason.NetworkStreamReadError));
        Debug.WriteLine($"Error reading network stream from {RemoteIpAddress}: {ex.Message}");
        Disconnect(Reason.NetworkStreamReadError);
      }
    }
  }


  protected override void HandleOutgoingPackets() {
    while (CanRead && !ServerCancellationToken.IsCancellationRequested) {
      var packetExists = OutgoingPacketQueue.TryDequeue(out var packet);
      if (packetExists)
        try {
          Debug.WriteLine($"Sending packet to {RemoteIpAddress} with message id {packet.MessageId}");
          
          if (packet.Encrypted) {
            var providerExists = Scope.TryResolve<INetTcpEncryptionProvider>(out var provider);
            if (!providerExists) {
              Debug.WriteLine("Encryption provider not found", "NetTcpClient");
              _server.InvokeConnectionError(new ConnectionErrorEventArgs(this, new Exception("Encryption provider not found, packet impossible to send"), Reason.EncryptionProviderNotFound));
              continue;
            }

            var encrypted = provider.Encrypt(packet.Body);
            BinaryWriter.Write(packet.MessageId);
            BinaryWriter.Write(packet.Encrypted);
            BinaryWriter.Write(packet.Size);
            BinaryWriter.Write(encrypted);
          }
          else {
            BinaryWriter.Write(packet.MessageId);
            BinaryWriter.Write(packet.Encrypted);
            BinaryWriter.Write(packet.Size);
            BinaryWriter.Write(packet.Body);
          }
        }
        catch (Exception ex) {
          _server.InvokeConnectionError(new ConnectionErrorEventArgs(this, ex, Reason.PacketSendQueueError));
          Debug.WriteLine($"Error sending packet to {RemoteIpAddress} with message id {packet.MessageId}: {ex.Message}");
        }
    }
  }


  protected override void HandleIncomingPackets() {
    while (!ServerCancellationToken.IsCancellationRequested) {
      if (IncomingPacketQueue.TryDequeue(out var packet)) {
        var handlerExists = _server.PacketManager.TryGetMessageHandler(packet.MessageId, out var handler);
        if (!handlerExists) {
          _server.InvokeMessageHandlerNotFound(new MessageHandlerNotFoundEventArgs(this, packet));
          Debug.WriteLine($"Message handler not found for packet from {RemoteIpAddress} with message id {packet.MessageId}");
          continue;
        }

        try {
          Debug.WriteLine($"Handling packet from {RemoteIpAddress} with message id {packet.MessageId}");
          RunningHandler = true;
          handler.Invoke(this, packet.Message);
          RunningHandler = false;
          Debug.WriteLine($"Packet handled from {RemoteIpAddress} with message id {packet.MessageId}");
        }
        catch (Exception ex) {
          Debug.WriteLine($"Handler error from {RemoteIpAddress} Message:{ex.Message}");
          _server.InvokeHandlerError(new HandlerErrorEventArgs(this, ex));
        }
      }
    }
  }


  /// <summary>
  ///   Disconnects the client from the server
  ///   However before it waits current handlers to send the responses
  ///   But it will stop accepting new request messages
  /// </summary>
  /// <param name="reason"></param>
  public override void Disconnect(Reason reason = Reason.Unknown) {
    try {
      base.ProcessDisconnect(reason);
      _server.InvokeClientDisconnected(new ClientDisconnectedEventArgs(this, reason));
      Debug.WriteLine($"Client disconnected: {RemoteIpAddress}");
    }
    catch (Exception ex) {
      _server.InvokeConnectionError(new ConnectionErrorEventArgs(this, ex, reason));
      Debug.WriteLine($"Error disconnecting client: {ex.Message}");
    }
  }

  public void EnqueuePacketSend(IPacket message,
                                bool encrypted = false) {
    if (!_server.PacketManager.GetOpcode(message, out var opcode)) {
      _server.InvokeUnknownPacketSendAttempt(new UnknownPacketSendAttemptEventArgs(this, message, encrypted));
      Debug.WriteLine($"Unknown packet send attempted: {message}");
      return;
    }
    base.ProcessOutgoingPacket(opcode, encrypted, message);
    _server.InvokePacketQueued(new PacketQueuedEventArgs(this, opcode, encrypted,PacketQueueType.Outgoing));
    Debug.WriteLine($"Packet queued: {message}");
  }
}