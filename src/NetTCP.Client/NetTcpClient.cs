using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using NetTCP.Abstract;
using NetTCP.Client.Events;
using NetTCP.Network;

namespace NetTCP.Client;

public class NetTcpClient : IDisposable
{
  public string Host { get; }
  public ushort Port { get; }
  protected ConcurrentQueue<ProcessedIncomingPacket> IncomingPacketQueue { get; } = new();
  protected ConcurrentQueue<ProcessedOutgoingPacket> OutgoingPacketQueue { get; } = new();
  protected ISerializer Serializer { get; }
  protected NetTcpClientPacketContainer PacketContainer { get; }


  protected TcpClient Client { get; }
  protected CancellationToken ServerCancellationToken { get; }
  protected CancellationTokenSource ClientCancellationTokenSource { get; }

  public bool CanProcess => Client.Connected && !ClientCancellationTokenSource.IsCancellationRequested;
  public bool AnyPacketsProcessing => !(IncomingPacketQueue.IsEmpty && OutgoingPacketQueue.IsEmpty);

  public long LastActivity { get; private set; }

  protected NetworkStream NetworkStream => Client.GetStream();

  protected BinaryWriter BinaryWriter {
    get {
      if (!NetworkStream.CanWrite) throw new Exception("Cannot write to the stream");

      return new BinaryWriter(NetworkStream, Encoding.UTF8, true);
    }
  }

  protected BinaryReader BinaryReader {
    get {
      if (!NetworkStream.CanRead) throw new Exception("Cannot read from the stream");

      return new BinaryReader(NetworkStream, Encoding.UTF8, true);
    }
  }


  internal NetTcpClient(string host, ushort port, NetTcpClientPacketContainer packetContainer, ISerializer serializer) {
    Host = host;
    Port = port;
    Client = new TcpClient();
    ClientCancellationTokenSource = new CancellationTokenSource();
    PacketContainer = packetContainer;
    Serializer = serializer;
  }

  public event EventHandler<ClientConnectedEventArgs> ClientConnected;
  public event EventHandler<ConnectionErrorEventArgs> ConnectionError;
  public event EventHandler<ClientDisconnectedEventArgs> ClientDisconnected;
  public event EventHandler<UnknownPacketReceivedEventArgs> UnknownPacketReceived;
  public event EventHandler<UnknownPacketSendAttemptEventArgs> UnknownPacketSendAttempted;
  public event EventHandler<MessageHandlerNotFoundEventArgs> MessageHandlerNotFound;
  public event EventHandler<PacketQueuedEventArgs> PacketQueued;
  public event EventHandler<PacketReceivedEventArgs> PacketReceived;


  public void Connect() {
    Client.Connect(Host, Port);
    ClientConnected?.Invoke(this, new ClientConnectedEventArgs(this));
    Task.Run(HandleConnectionTask, ClientCancellationTokenSource.Token);
    Task.Run(HandleOutgoingPacketQueue, ClientCancellationTokenSource.Token);
    Task.Run(HandleIncomingPacketQueue, ClientCancellationTokenSource.Token);
  }

  /// <summary>
  /// Tries to connect to the server with a retry count and a delay.
  ///
  /// If retryCount is 0 it will try to connect indefinitely
  /// </summary>
  /// <param name="retryCount"></param>
  /// <exception cref="Exception"></exception>
  public void ConnectWithRetry(uint retryCount = 3, int retryDelayMs = 1000) {
    var retry = 0;
    while (true) {
      try {
        Connect();
        return;
      }
      catch (Exception ex) {
        retry++;
        if (retry == retryCount) {
          ConnectionError?.Invoke(this, new ConnectionErrorEventArgs(this, ex, Reason.ConnectionFailed));
          throw new Exception("Could not connect to server", ex);
        }

        Thread.Sleep(retryDelayMs);
      }
    }
  }


  private async Task HandleOutgoingPacketQueue() {
    while (CanProcess) {
      var packetExists = OutgoingPacketQueue.TryDequeue(out var packet);
      if (packetExists) {
        try {
          PacketQueued?.Invoke(this, new PacketQueuedEventArgs(this, packet.MessageId, packet.Encrypted));
          BinaryWriter.Write(packet.MessageId);
          BinaryWriter.Write(packet.Encrypted);
          BinaryWriter.Write(packet.Size);
          BinaryWriter.Write(packet.Body);
        }
        catch (Exception ex) {
          ConnectionError?.Invoke(this, new ConnectionErrorEventArgs(this, ex, Reason.PacketSendQueueError));
          Disconnect(Reason.PacketSendQueueError);
        }
      }
    }
  }

  private async Task HandleIncomingPacketQueue() {
    while (CanProcess) {
      var packetExists = IncomingPacketQueue.TryDequeue(out var packet);
      if (packetExists)
        try {
          var result = PacketContainer.InvokeHandler(packet.MessageId, this, packet.Message);
          if (!result) {
            MessageHandlerNotFound?.Invoke(this, new MessageHandlerNotFoundEventArgs(this, packet));
          }
        }
        catch (Exception ex) {
          ConnectionError?.Invoke(this, new ConnectionErrorEventArgs(this, ex, Reason.PacketInvokeHandlerError));
          Disconnect(Reason.PacketInvokeHandlerError);
        }
    }
  }

  public void EnqueuePacketSend(IPacket message,
                                bool encrypted = false) {
    if (!PacketContainer.GetOpcode(message, out var opcode)) {
      UnknownPacketSendAttempted?.Invoke(this, new UnknownPacketSendAttemptEventArgs(this, message, encrypted));
      return;
    }

    var bytes = Serializer.Serialize(message);
    var packet = new ProcessedOutgoingPacket(opcode, encrypted, bytes);
    OutgoingPacketQueue.Enqueue(packet);
  }


  private void HandleReceivePacket(int messageId, bool encrypted, int size, byte[] restBytes) {
    Task.Run(() => {
      try {
        var messageInstance = PacketContainer.GetMessage(messageId);
        if (messageInstance == null) {
          UnknownPacketReceived?.Invoke(this, new UnknownPacketReceivedEventArgs(this, messageId, encrypted, size, restBytes));
          return;
        }

        var data = Serializer.Deserialize(messageInstance, restBytes);
        var clientPacket = new ProcessedIncomingPacket(messageId, encrypted, (IPacket)data);
        IncomingPacketQueue.Enqueue(clientPacket);
      }
      catch (Exception ex) {
        ConnectionError?.Invoke(this, new ConnectionErrorEventArgs(this, ex, Reason.PacketReceiveHandleError));
        Disconnect(Reason.PacketReceiveHandleError);
      }
    });
  }

  private void HandleConnectionTask() {
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
        ConnectionError?.Invoke(this, new ConnectionErrorEventArgs(this, ex, Reason.NetworkStreamReadError));
        Disconnect(Reason.NetworkStreamReadError);
      }
    }

    Disconnect(Reason.ConnectionClosed);
  }

  /// <summary>
  /// Disconnects and disposes the client. This method will trigger the ClientDisconnected event.
  /// </summary>
  /// <param name="reason"></param>
  public void Disconnect(Reason reason) {
    if (!CanProcess) return;
    try {
      ClientDisconnected?.Invoke(this, new ClientDisconnectedEventArgs(this, reason));
      Dispose();
    }
    catch (Exception ex) {
      ConnectionError?.Invoke(this, new ConnectionErrorEventArgs(this, ex, reason));
    }
  }

  /// <summary>
  /// Disconnects and disposes the client. This method will not trigger any events.
  /// </summary>
  public void Dispose() {
    ClientCancellationTokenSource.Cancel();
    ClientCancellationTokenSource.Dispose();
    Client.Close();
    Client.Dispose();
  }
}