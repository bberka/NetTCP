using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using NetTCP.Abstract;
using NetTCP.Network;

namespace NetTCP.Client;

public class NetTcpClient
{
  private readonly ConcurrentQueue<ProcessedIncomingPacket> _incomingPacketQueue = new();
  private readonly ConcurrentQueue<ProcessedOutgoingPacket> _outgoingPacketQueue = new();
  private readonly ISerializer _serializer;
  protected readonly NetTcpClientPacketContainer PacketContainer;


  internal NetTcpClient(string host, ushort port, NetTcpClientPacketContainer packetContainer, ISerializer serializer) {
    Client = new TcpClient(host, port);
    ClientCancellationTokenSource = new CancellationTokenSource();
    PacketContainer = packetContainer;
    _serializer = serializer;
  }

  public TcpClient Client { get; set; }

  protected CancellationToken ServerCancellationToken { get; }
  protected CancellationTokenSource ClientCancellationTokenSource { get; }

  public bool CanProcess => Client.Connected && !ClientCancellationTokenSource.IsCancellationRequested;
  public bool AnyPacketsProcessing => !(_incomingPacketQueue.IsEmpty && _outgoingPacketQueue.IsEmpty);

  public long LastActivity { get; set; }

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


  public void Connect() {
    Task.Run(HandleConnectionTask, ClientCancellationTokenSource.Token);
    Task.Run(OutgoingPacketQueueHandlerTask, ClientCancellationTokenSource.Token);
    Task.Run(IncomingPacketQueueHandlerTask, ClientCancellationTokenSource.Token);
  }

  private async Task OutgoingPacketQueueHandlerTask() {
    try {
      while (CanProcess) {
        var packetExists = _outgoingPacketQueue.TryDequeue(out var packet);
        if (packetExists) {
          BinaryWriter.Write(packet.MessageId);
          BinaryWriter.Write(packet.Encrypted);
          BinaryWriter.Write(packet.Size);
          BinaryWriter.Write(packet.Body);
        }
      }
    }
    catch (Exception ex) {
      // Handle exceptions, log, or notify as needed
    }
  }

  private async Task IncomingPacketQueueHandlerTask() {
    try {
      while (CanProcess) {
        var packetExists = _incomingPacketQueue.TryDequeue(out var packet);
        if (packetExists)
          try {
            PacketContainer.InvokeHandler(packet.MessageId, this, packet.Message);
          }
          catch (Exception ex) {
            //TODO ? 
            Disconnect(DisconnectReason.InvalidPacket);
          }
      }
    }
    catch (Exception ex) {
      // Handle exceptions, log, or notify as needed
      //TODO
    }
  }

  public void EnqueuePacketSend(IPacket message,
                                bool encrypted = false) {
    if (!PacketContainer.GetOpcode(message, out var opcode))
      //invalid message opcode
      //TODO Trigger event
      return;
    var bytes = _serializer.Serialize(message);
    var packet = new ProcessedOutgoingPacket(opcode, encrypted, bytes);
    _outgoingPacketQueue.Enqueue(packet);
  }


  private void HandleReceivePacket(int messageId, bool encrypted, int size, byte[] restBytes) {
    Task.Run(() => {
      var messageInstance = PacketContainer.GetMessage(messageId);
      if (messageInstance == null)
        //invalid message id
        //TODO Trigger event
        return;

      var data = _serializer.Deserialize(messageInstance, restBytes);
      var clientPacket = new ProcessedIncomingPacket(messageId, encrypted, (IPacket)data);
      _incomingPacketQueue.Enqueue(clientPacket);
    });
  }

  private void HandleConnectionTask() {
    try {
      while (CanProcess) {
        ServerCancellationToken.ThrowIfCancellationRequested();
        LastActivity = DateTime.Now.Ticks;
        var messageId = BinaryReader.ReadInt32();
        var encrypted = BinaryReader.ReadBoolean();
        var size = BinaryReader.ReadInt32();
        var restBytes = BinaryReader.ReadBytes(size);
        Console.WriteLine($"Received packet with id {messageId} and size {size} ");
        HandleReceivePacket(messageId, encrypted, size, restBytes);
      }
    }
    catch (Exception ex) {
      //TODO Trigger event
    }
    finally {
      Disconnect(DisconnectReason.Unknown);
    }
  }

  private void Disconnect(DisconnectReason unknown) { }
}