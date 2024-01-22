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
  private bool Disposing = false;
  protected ConcurrentQueue<ProcessedIncomingPacket> IncomingPacketQueue { get; } = new();
  protected ConcurrentQueue<ProcessedOutgoingPacket> OutgoingPacketQueue { get; } = new();
  protected ISerializer Serializer { get; }
  protected NetTcpServerPacketContainer PacketContainer { get; }


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


  public NetTcpConnection(TcpClient client, NetTcpServerPacketContainer packetContainer, CancellationToken serverCancellationToken, ISerializer serializer) {
    PacketContainer = packetContainer;
    Serializer = serializer;
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
      var packetExists = OutgoingPacketQueue.TryDequeue(out var packet);
      if (packetExists)
        try {
          BinaryWriter.Write(packet.MessageId);
          BinaryWriter.Write(packet.Encrypted);
          BinaryWriter.Write(packet.Size);
          BinaryWriter.Write(packet.Body);
        }
        catch (Exception ex) {
          ConnectionError?.Invoke(this, new ConnectionErrorEventArgs(this, ex, Reason.PacketSendQueueError));
          DisconnectByServer(Reason.PacketSendQueueError);
        }
    }
  }

  private void HandleIncomingPacketQueue() {
    while (CanProcess) {
      var packetExists = IncomingPacketQueue.TryDequeue(out var packet);
      if (packetExists)
        try {
          var result = PacketContainer.InvokeHandler(packet.MessageId, this, packet.Message);
          if (result == false) {
            MessageHandlerNotFound?.Invoke(this, new MessageHandlerNotFoundEventArgs(this, packet));
          }
        }
        catch (Exception ex) {
          ConnectionError?.Invoke(this, new ConnectionErrorEventArgs(this, ex, Reason.PacketInvokeHandlerError));
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
      Dispose();
    }
    catch (Exception ex) {
      ConnectionError?.Invoke(this, new ConnectionErrorEventArgs(this, ex, reason));
    }
  }

  public void EnqueuePacketSend(IPacket message,
                                bool encrypted = false) {
    if (!PacketContainer.GetOpcode(message, out var opcode)) {
      UnknownPacketSendAttempted?.Invoke(this, new UnknownPacketSendAttemptEventArgs(this, message, encrypted));
      return;
    }

    var bytes = Serializer.Serialize(message);
    var serverPacket = new ProcessedOutgoingPacket(opcode, encrypted, bytes);
    OutgoingPacketQueue.Enqueue(serverPacket);
  }


  private void HandleReceivePacket(int messageId, bool encrypted, int size, byte[] restBytes) {
    Task.Run(() => {
      var messageInstance = PacketContainer.GetMessage(messageId);
      if (messageInstance == null) {
        UnknownPacketReceived?.Invoke(this, new UnknownPacketReceivedEventArgs(this, messageId, encrypted, size, BinaryReader.ReadBytes(size)));
        return;
      }

      var data = Serializer.Deserialize(messageInstance, restBytes);
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
        Console.WriteLine($"Received packet with id {messageId} and size {size} from {RemoteIpAddress}:{RemotePort}.");
        HandleReceivePacket(messageId, encrypted, size, restBytes);
      }
      catch (Exception ex) {
        ConnectionError?.Invoke(this, new ConnectionErrorEventArgs(this, ex, Reason.NetworkStreamReadError));
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