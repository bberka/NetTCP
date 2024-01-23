using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NetTCP.Abstract;
using NetTCP.Network;
using NetTCP.Server.Events;

namespace NetTCP.Server;

public class NetTcpConnection : IDisposable
{
  private readonly NetTcpServer _server;
  private bool Disposing = false;
  protected ConcurrentQueue<ProcessedIncomingPacket> IncomingPacketQueue { get; } = new();
  protected ConcurrentQueue<ProcessedOutgoingPacket> OutgoingPacketQueue { get; } = new();

  protected TcpClient Client { get; }
  protected CancellationToken ServerCancellationToken { get; }
  protected CancellationTokenSource ClientCancellationTokenSource { get; }

  public bool CanProcess => Client.Connected && !ClientCancellationTokenSource.IsCancellationRequested;

  public bool AnyPacketsProcessing => !(IncomingPacketQueue.IsEmpty && OutgoingPacketQueue.IsEmpty);
  public long LastActivity { get; private set; }

  protected NetworkStream NetworkStream => Client.GetStream();


  protected BinaryReader BinaryReader {
    get {
      if (!NetworkStream.CanRead) throw new Exception("Cannot read from the stream");

      return new BinaryReader(NetworkStream, Encoding.UTF8, true);
    }
  }

  protected BinaryWriter BinaryWriter {
    get {
      if (!NetworkStream.CanWrite) throw new Exception("Cannot write to the stream");

      return new BinaryWriter(NetworkStream, Encoding.UTF8, true);
    }
  }

  public ushort RemotePort { get; set; }

  public IPAddress RemoteIpAddress { get; set; }


  public NetTcpConnection(TcpClient client, NetTcpServer server, CancellationToken serverCancellationToken) {
    _server = server;
    Client = client;
    ServerCancellationToken = serverCancellationToken;
    ClientCancellationTokenSource = new CancellationTokenSource();
    IncomingPacketQueue = new ConcurrentQueue<ProcessedIncomingPacket>();
    OutgoingPacketQueue = new ConcurrentQueue<ProcessedOutgoingPacket>();
    RemoteIpAddress = ((IPEndPoint)Client.Client.RemoteEndPoint).Address;
    RemotePort = (ushort)((IPEndPoint)Client.Client.RemoteEndPoint).Port;
    _ = Task.Run(HandleConnection, ClientCancellationTokenSource.Token);
    _ = Task.Run(HandleIncomingPacketQueue, ClientCancellationTokenSource.Token);
    _ = Task.Run(HandleOutgoingPacketQueue, ClientCancellationTokenSource.Token);
  }


  private void HandleOutgoingPacketQueue() {
    while (CanProcess) {
      var packetExists = OutgoingPacketQueue.TryDequeue(out var packet);
      if (packetExists)
        try {
          BinaryWriter.Write(packet.MessageId);
          BinaryWriter.Write(packet.Encrypted);
          BinaryWriter.Write(packet.Size);
          BinaryWriter.Write(packet.Body);
        }
        catch (Exception ex) {
          _server.InvokeConnectionError(new ConnectionErrorEventArgs(this, ex, Reason.PacketSendQueueError));
          DisconnectByServer(Reason.PacketSendQueueError);
        }
    }
  }

  private void HandleIncomingPacketQueue() {
    while (CanProcess) {
      var packetExists = IncomingPacketQueue.TryDequeue(out var packet);
      if (packetExists)
        try {
          var result = _server.PacketContainer.InvokeHandler(packet.MessageId, this, packet.Message);
          if (result == false) {
            _server.InvokeMessageHandlerNotFound(new MessageHandlerNotFoundEventArgs(this, packet));
          }
        }
        catch (Exception ex) {
          _server.InvokeConnectionError(new ConnectionErrorEventArgs(this, ex, Reason.PacketInvokeHandlerError));
          DisconnectByServer(Reason.PacketInvokeHandlerError);
        }
    }
  }


  /// <summary>
  ///   Disconnects the client from the server
  ///   However before it waits current handlers to send the responses
  ///   But it will stop accepting new request messages
  /// </summary>
  /// <param name="reason"></param>
  public void DisconnectByServer(Reason reason = Reason.Unknown) {
    if (!CanProcess) return;
    try {
      _server.InvokeClientDisconnected(new ClientDisconnectedEventArgs(this, reason));
      Dispose();
    }
    catch (Exception ex) {
      _server.InvokeConnectionError(new ConnectionErrorEventArgs(this, ex, reason));
    }
  }

  public void EnqueuePacketSend(IPacket message,
                                bool encrypted = false) {
    if (!_server.PacketContainer.GetOpcode(message, out var opcode)) {
      _server.InvokeUnknownPacketSendAttempt(new UnknownPacketSendAttemptEventArgs(this, message, encrypted));
      return;
    }

    _server.InvokePacketQueued(new PacketQueuedEventArgs(this, opcode, encrypted));
    var bytes = _server.Serializer.Serialize(message);
    var serverPacket = new ProcessedOutgoingPacket(opcode, encrypted, bytes);
    OutgoingPacketQueue.Enqueue(serverPacket);
  }


  private void HandleReceivePacket(int messageId, bool encrypted, int size, byte[] restBytes) {
    Task.Run(() => {
      var messageInstance = _server.PacketContainer.GetMessage(messageId);
      if (messageInstance == null) {
        _server.InvokeUnknownPacketReceived(new UnknownPacketReceivedEventArgs(this, messageId, encrypted, size, restBytes));
        return;
      }

      var data = _server.Serializer.Deserialize(messageInstance, restBytes);
      var clientPacket = new ProcessedIncomingPacket(messageId, encrypted, (IPacket)data);
      IncomingPacketQueue.Enqueue(clientPacket);
    });
  }

  private async void HandleConnection() {
    while (CanProcess) {
      try {
        ServerCancellationToken.ThrowIfCancellationRequested();
        LastActivity = DateTime.Now.Ticks;
        var messageId = BinaryReader.ReadInt32();
        var encrypted = BinaryReader.ReadBoolean();
        var size = BinaryReader.ReadInt32();
        var restBytes = BinaryReader.ReadBytes(size);
        HandleReceivePacket(messageId, encrypted, size, restBytes);
      }
      catch (Exception ex) {
        _server.InvokeConnectionError(new ConnectionErrorEventArgs(this, ex, Reason.NetworkStreamReadError));
        DisconnectByServer(Reason.NetworkStreamReadError);
      }
    }

    DisconnectByServer();
  }

  public void Dispose() {
    ClientCancellationTokenSource.Cancel();
    ClientCancellationTokenSource.Dispose();
    Client.Dispose();
  }
}