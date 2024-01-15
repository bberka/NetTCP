using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NetTCP.Abstract;
using NetTCP.Serialization;
using NetTCP.Server.Events;
using NetTCP.Server.Model;

namespace NetTCP.Server;

public sealed class NetTcpConnection : IDisposable
{
  private readonly NetServerPacketContainer _packetContainer;
  private PacketReader _packetReader;
  private PacketWriter _packetWriter;
  protected TcpClient Client { get; }
  protected CancellationToken ServerCancellationToken { get; }
  protected CancellationTokenSource ClientCancellationTokenSource { get; }

  private ConcurrentQueue<ProcessedClientPacket> _incomingPacketQueue;
  private ConcurrentQueue<ProcessedServerPacket> _outgoingPacketQueue;

  public bool CanProcess => Client.Connected && !ClientCancellationTokenSource.IsCancellationRequested;

  public bool AnyProcessingPackets => !_incomingPacketQueue.IsEmpty || _outgoingPacketQueue.IsEmpty;


  private bool Disposing = false;
  public long LastActivity { get; set; }


  protected PacketReader PacketReader {
    get {
      if (_packetReader == null) {
        lock (_packetReader) {
          if (_packetReader == null) {
            var stream = Client.GetStream();
            var reader = new BinaryReader(stream, Encoding.UTF8, true);
            _packetReader = new PacketReader(reader);
          }
        }
      }

      return _packetReader;
    }
  }

  protected PacketWriter PacketWriter {
    get {
      if (_packetWriter == null) {
        lock (_packetWriter) {
          if (_packetWriter == null) {
            var stream = Client.GetStream();
            var writer = new BinaryWriter(stream, Encoding.UTF8, true);
            _packetWriter = new PacketWriter(writer);
          }
        }
      }

      return _packetWriter;
    }
  }

  public ushort RemotePort { get; set; }

  public IPAddress RemoteIpAddress { get; set; }


  public NetTcpConnection(TcpClient client, NetServerPacketContainer packetContainer, CancellationToken serverCancellationToken) {
    _packetContainer = packetContainer;
    Client = client;
    ServerCancellationToken = serverCancellationToken;
    ClientCancellationTokenSource = new CancellationTokenSource();
    _incomingPacketQueue = new ConcurrentQueue<ProcessedClientPacket>();
    _outgoingPacketQueue = new ConcurrentQueue<ProcessedServerPacket>();
    RemoteIpAddress = ((IPEndPoint)Client.Client.RemoteEndPoint).Address;
    RemotePort = (ushort)((IPEndPoint)Client.Client.RemoteEndPoint).Port;
    _ = Task.Run(HandleConnection, ClientCancellationTokenSource.Token);
    _ = Task.Run(HandleIncomingPacketQueue, ClientCancellationTokenSource.Token);
    _ = Task.Run(HandleOutgoingPacketQueue, ClientCancellationTokenSource.Token);
  }


  private event EventHandler<ConnectionErrorEventArgs> ConnectionError;
  private event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;
  private event EventHandler<UnknownPacketReceivedEventArgs> UnknownPacketReceived;
  private event EventHandler<UnknownPacketSendAttemptEventArgs> UnknownPacketSendAttempted;
  private event EventHandler<MessageHandlerNotFoundEventArgs> MessageHandlerNotFound;
  private event EventHandler<PacketQueuedEventArgs> PacketQueued;
  private event EventHandler<PacketReceivedEventArgs> PacketReceived;

  internal void SubscribeToEvents(NetTcpServer netTcpServer) {
    netTcpServer.ConnectionError += ConnectionError;
    netTcpServer.ClientDisconnected += ClientDisconnected;
    netTcpServer.UnknownPacketReceived += UnknownPacketReceived;
    netTcpServer.UnknownPacketSendAttempted += UnknownPacketSendAttempted;
    netTcpServer.MessageHandlerNotFound += MessageHandlerNotFound;
    netTcpServer.PacketQueued += PacketQueued;
    netTcpServer.PacketReceived += PacketReceived;
  }


  private void HandleOutgoingPacketQueue() {
    while (CanProcess) {
      if (!_outgoingPacketQueue.TryDequeue(out var packet) && packet != null) {
        try {
          PacketWriter.Write(packet.MessageId);
          PacketWriter.Write(packet.Encrypted);
          PacketWriter.Write(packet.Size);
          PacketWriter.Write(packet.Data);
          PacketWriter.Flush();
        }
        catch (Exception ex) {
          ConnectionError?.Invoke(this, new ConnectionErrorEventArgs(this, ex));
          DisconnectByServer(DisconnectReason.PacketTransmissionError);
        }
      }
    }
  }

  private void HandleIncomingPacketQueue() {
    while (CanProcess) {
      if (!_incomingPacketQueue.TryDequeue(out var packet) && packet != null) {
        try {
          _packetContainer.InvokeHandler(packet.MessageId, this, packet.Message);
        }
        catch (Exception ex) {
          ConnectionError?.Invoke(this, new ConnectionErrorEventArgs(this, ex));
          DisconnectByServer(DisconnectReason.InvalidPacket);
        }
      }
    }
  }


  /// <summary>
  /// Disconnects the client from the server
  /// However before it waits current handlers to send the responses
  /// But it will stop accepting new request messages
  /// </summary>
  /// <param name="reason"></param>
  public void DisconnectByServer(DisconnectReason reason = DisconnectReason.Unknown) {
    if (!CanProcess) {
      return;
    }

    try {
      Dispose();
    }
    catch (Exception ex) {
      ConnectionError?.Invoke(this, new ConnectionErrorEventArgs(this, ex));
    }
  }

  public void EnqueuePacketSend(IPacketWriteable message,
                                bool encrypted = false) {
    if (!_packetContainer.GetOpcode(message, out var opcode)) {
      UnknownPacketSendAttempted?.Invoke(this, new UnknownPacketSendAttemptEventArgs(this, message, encrypted));
      return;
    }

    var serverPacket = new ProcessedServerPacket(opcode, encrypted, message);
    _outgoingPacketQueue.Enqueue(serverPacket);
  }


  private void HandleReceivePacket(int messageId, bool encrypted, int size) {
    Task.Run((() => {
                 var messageInstance = _packetContainer.GetMessage(messageId);
                 if (messageInstance == null) {
                   UnknownPacketReceived?.Invoke(this, new UnknownPacketReceivedEventArgs(this, messageId, encrypted, size, PacketReader.ReadBytes(size)));
                   return;
                 }

                 messageInstance.Read(PacketReader);
                 var clientPacket = new ProcessedClientPacket(messageId, encrypted,messageInstance);
                 _incomingPacketQueue.Enqueue(clientPacket);
               }));
  }

  private async void HandleConnection() {
    try {
      while (CanProcess) {
        ServerCancellationToken.ThrowIfCancellationRequested();
        LastActivity = DateTime.Now.Ticks;
        var messageId = PacketReader.ReadInt32();
        var encrypted = PacketReader.ReadBoolean();
        var size = PacketReader.ReadInt32();
        HandleReceivePacket(messageId, encrypted, size);
      }
    }
    catch (Exception ex) {
      ConnectionError?.Invoke(this, new ConnectionErrorEventArgs(this, ex));
      DisconnectByServer(DisconnectReason.Unknown);
    }
  }

  public void Dispose() {
    ClientCancellationTokenSource.Dispose();
    Client.Dispose();
  }
}