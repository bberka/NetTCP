using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using Autofac;
using NetTCP.Abstract;
using NetTCP.Client.Events;
using NetTCP.Network;

namespace NetTCP.Client;

public class NetTcpClient : NetTcpConnectionBase
{
  protected NetTcpPacketManager<NetTcpClient> PacketManager { get; }
  public IContainer ServiceContainer { get; private set; }

  public NetTcpClient(string remoteIp, ushort remotePort, IContainer serviceContainer, Assembly[]? packetAssemblies = null) {
    if (string.IsNullOrEmpty(remoteIp)) {
      throw new ArgumentException("Empty ip address: " + remoteIp, nameof(remoteIp));
    }

    var parseIp = IPAddress.TryParse(remoteIp, out var ipAddress);
    if (parseIp == false)
      throw new ArgumentException("Invalid ip address: " + remoteIp, nameof(remoteIp));
    var isValidPort = NetTcpTools.IsValidPort(remotePort);
    if (isValidPort == false)
      throw new ArgumentException("Invalid port: " + remotePort, nameof(remotePort));

    if (serviceContainer is null) {
      throw new ArgumentNullException(nameof(serviceContainer));
    }

    PacketManager = new NetTcpPacketManager<NetTcpClient>();
    packetAssemblies ??= Array.Empty<Assembly>();
    foreach (var assembly in packetAssemblies) {
      PacketManager.Register(assembly);
    }

    PacketManager.Initialize();

    RemoteIpAddress = remoteIp;
    RemotePort = remotePort;
    Client = new TcpClient();
    CancellationTokenSource = new CancellationTokenSource();
    ConnectedAtUtc = DateTime.UtcNow;
    ServiceContainer = serviceContainer;
  }


  public event EventHandler<ClientConnectedEventArgs> ClientConnected;
  public event EventHandler<ConnectionErrorEventArgs> ConnectionError;
  public event EventHandler<HandlerErrorEventArgs> HandlerError;
  public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;
  public event EventHandler<UnknownPacketReceivedEventArgs> UnknownPacketReceived;
  public event EventHandler<UnknownPacketSendAttemptEventArgs> UnknownPacketSendAttempted;
  public event EventHandler<MessageHandlerNotFoundEventArgs> MessageHandlerNotFound;
  public event EventHandler<PacketQueuedEventArgs> PacketQueued;
  public event EventHandler<PacketReceivedEventArgs> PacketReceived;


  protected override void HandleStream() {
    while (CanRead) {
      try {
        Debug.WriteLine("Waiting for data " + RemotePort, "NetTcpClient");
        LastActivity = DateTime.Now.Ticks;
        var messageId = BinaryReader.ReadInt32();
        var encrypted = BinaryReader.ReadBoolean();
        var size = BinaryReader.ReadInt32();
        var restBytes = BinaryReader.ReadBytes(size);

        var messageInstance = PacketManager.GetMessage(messageId);
        if (messageInstance == null) {
          Debug.WriteLine("Unknown packet received: " + messageId, "NetTcpClient");
          UnknownPacketReceived?.Invoke(this, new UnknownPacketReceivedEventArgs(this, messageId, encrypted, size, restBytes));
          return;
        }

        if (encrypted) {
          var providerExists = Scope.TryResolve<INetTcpEncryptionProvider>(out var provider);
          if (!providerExists) {
            Debug.WriteLine("Encryption provider not found", "NetTcpClient");
            ConnectionError?.Invoke(this, new ConnectionErrorEventArgs(this, new Exception("Encryption provider not found"), Reason.EncryptionProviderNotFound));
            return;
          }

          var decrypted = provider.Decrypt(restBytes);
          restBytes = decrypted;
        }

        messageInstance.Read(new TcpPacketReader(restBytes));
        var clientPacket = new ProcessedIncomingPacket(messageId, encrypted, messageInstance);
        Debug.WriteLine("Packet received: " + messageId, "NetTcpClient");
        IncomingPacketQueue.Enqueue(clientPacket);
        PacketQueued?.Invoke(this, new PacketQueuedEventArgs(this, messageId, encrypted, PacketQueueType.Incoming));
      }
      catch (Exception ex) {
        Debug.WriteLine("Error reading network stream: " + ex.Message, "NetTcpClient");
        ConnectionError?.Invoke(this, new ConnectionErrorEventArgs(this, ex, Reason.NetworkStreamReadError));
      }
    }
  }


  protected override void HandleOutgoingPackets() {
    while (CanRead) {
      var packetExists = OutgoingPacketQueue.TryDequeue(out var packet);
      if (packetExists) {
        try {
          Debug.WriteLine($"Sending packet messageId:{packet.MessageId} encrypted:{packet.Encrypted} size:{packet.Size} to {RemoteIpAddress}:{RemotePort}", "NetTcpClient");
          BinaryWriter.Write(packet.MessageId);
          BinaryWriter.Write(packet.Encrypted);
          BinaryWriter.Write(packet.Size);
          BinaryWriter.Write(packet.Body);
        }
        catch (Exception ex) {
          ConnectionError?.Invoke(this, new ConnectionErrorEventArgs(this, ex, Reason.PacketSendQueueError));
          Debug.WriteLine($"Error sending packet messageId:{packet.MessageId} encrypted:{packet.Encrypted} size:{packet.Size} to {RemoteIpAddress}:{RemotePort} {ex.Message}", "NetTcpClient");
        }
      }
    }
  }

  protected override void HandleIncomingPackets() {
    while (CanRead) {
      if (IncomingPacketQueue.TryDequeue(out var packet)) {
        var handlerExists = PacketManager.TryGetMessageHandler(packet.MessageId, out var handler);
        if (!handlerExists) {
          Debug.WriteLine($"Message handler not found for packet messageId:{packet.MessageId} encrypted:{packet.Encrypted} from {RemoteIpAddress}:{RemotePort}", "NetTcpClient");
          MessageHandlerNotFound?.Invoke(this, new MessageHandlerNotFoundEventArgs(this, packet));
          continue;
        }

        try {
          Debug.WriteLine($"Handling packet messageId:{packet.MessageId} encrypted:{packet.Encrypted} from {RemoteIpAddress}:{RemotePort}", "NetTcpClient");
          RunningHandler = true;
          handler.Invoke(this, packet.Message);
          RunningHandler = false;
          Debug.WriteLine($"Packet handled messageId:{packet.MessageId} encrypted:{packet.Encrypted} from {RemoteIpAddress}:{RemotePort}", "NetTcpClient");
        }
        catch (Exception ex) {
          HandlerError?.Invoke(this, new HandlerErrorEventArgs(this, ex));
          Debug.WriteLine($"Error handling packet messageId:{packet.MessageId} encrypted:{packet.Encrypted} from {RemoteIpAddress}:{RemotePort} {ex.Message}", "NetTcpClient");
        }
      }
    }
  }

  /// <summary>
  /// Disconnects and disposes the client. This method will trigger the ClientDisconnected event.
  /// </summary>
  /// <param name="reason"></param>
  public override void Disconnect(Reason reason = Reason.Unknown) {
    try {
      base.ProcessDisconnect(reason);
      Debug.WriteLine("Client disconnected: " + RemoteIpAddress, "NetTcpClient");
      ClientDisconnected?.Invoke(this, new ClientDisconnectedEventArgs(this, reason));
    }
    catch (Exception ex) {
      ConnectionError?.Invoke(this, new ConnectionErrorEventArgs(this, ex, reason));
      Debug.WriteLine("Error disconnecting client: " + ex.Message, "NetTcpClient");
    }
  }

  public void EnqueuePacketSend(IPacket message,
                                bool encrypted = false) {
    if (!PacketManager.GetOpcode(message, out var opcode)) {
      UnknownPacketSendAttempted?.Invoke(this, new UnknownPacketSendAttemptEventArgs(this, message, encrypted));
      Debug.WriteLine("Unknown packet send attempted: " + message, "NetTcpClient");
      return;
    }

    base.ProcessOutgoingPacket(opcode, encrypted, message);
    PacketQueued?.Invoke(this, new PacketQueuedEventArgs(this, opcode, encrypted, PacketQueueType.Outgoing));
    Debug.WriteLine("Packet queued: " + message, "NetTcpClient");
  }


  public void Connect() {
    Debug.WriteLine($"Connecting to {RemoteIpAddress}:{RemotePort}", "NetTcpClient");
    Client.Connect(RemoteIpAddress, RemotePort);
    Scope = ServiceContainer.BeginLifetimeScope();
    Task.Run(HandleStream, CancellationTokenSource.Token);
    Task.Run(HandleOutgoingPackets, CancellationTokenSource.Token);
    Task.Run(HandleIncomingPackets);
    Debug.WriteLine($"Connected to {RemoteIpAddress}:{RemotePort}", "NetTcpClient");
    ClientConnected?.Invoke(this, new ClientConnectedEventArgs(this));
  }
}