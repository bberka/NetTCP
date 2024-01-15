using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NetTCP.Abstract;
using NetTCP.Client.Model;
using NetTCP.Serialization;

namespace NetTCP.Client;

public class NetTcpClient
{
  protected readonly NetTcpClientPacketContainer PacketContainer;
  public TcpClient Client { get; set; }
  private readonly ConcurrentQueue<ProcessedClientPacket> _outgoingPacketQueue = new();
  private readonly ConcurrentQueue<ProcessedServerPacket> _incomingPacketQueue = new();

  private PacketReader _packetReader;
  private PacketWriter _packetWriter;
  protected CancellationToken ServerCancellationToken { get; }
  protected CancellationTokenSource ClientCancellationTokenSource { get; }

  public bool CanProcess => Client.Connected && !ClientCancellationTokenSource.IsCancellationRequested;
  public bool AnyPacketsProcessing => !(_incomingPacketQueue.IsEmpty && _outgoingPacketQueue.IsEmpty);

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

  

  internal NetTcpClient(string host, ushort port, NetTcpClientPacketContainer packetContainer) {
    Client = new TcpClient(host, port);
    ClientCancellationTokenSource = new CancellationTokenSource();
    PacketContainer = packetContainer;
  }


  public void Connect() {
    Task.Run(HandleConnectionTask,ClientCancellationTokenSource.Token);
    Task.Run(OutgoingPacketQueueHandlerTask,ClientCancellationTokenSource.Token);
    Task.Run(IncomingPacketQueueHandlerTask,ClientCancellationTokenSource.Token);
  }

  private async Task OutgoingPacketQueueHandlerTask() {
    try {
      while (CanProcess) {
        var packetExists = _outgoingPacketQueue.TryDequeue(out var packet);
        if (packetExists) {
          if(!PacketWriter.CanWrite)
             throw new Exception("Cannot write to stream");
          PacketWriter.Write(packet.MessageId);
          PacketWriter.Write(packet.Encrypted);
          PacketWriter.Write(packet.Size);
          PacketWriter.Write(packet.Data);
          PacketWriter.Flush();
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
        if (packetExists) {
          try {
            PacketContainer.InvokeHandler(packet.MessageId,this,packet.Message);
          }
          catch (Exception ex) {
            //TODO ? 
            Disconnect(DisconnectReason.InvalidPacket);
          }
          
          
        }
      }
    }
    catch (Exception ex) {
      // Handle exceptions, log, or notify as needed
       //TODO
    }
  }

  public void EnqueuePacketSend(IWriteablePacket message,
                                bool encrypted = false) {
    if (!PacketContainer.GetOpcode(message, out var opcode)) {
      //invalid message opcode
      //TODO Trigger event
      
      return;
    }
    var serverPacket = new ProcessedClientPacket(opcode, message, encrypted);
    _outgoingPacketQueue.Enqueue(serverPacket);
  }


  private void HandleReceivePacket(int messageId, bool encrypted, int size) {
    Task.Run((() => {
                 var messageInstance = PacketContainer.GetMessage(messageId);
                 if (messageInstance == null) {
                   //invalid message id
                   //TODO Trigger event
                   return;
                 }
                 messageInstance.Read(PacketReader);
                 var clientPacket = new ProcessedServerPacket(messageId, messageInstance, encrypted);
                 _incomingPacketQueue.Enqueue(clientPacket);
               }));
  }

  private void HandleConnectionTask() {
    try {
      while (CanProcess) {
        ServerCancellationToken.ThrowIfCancellationRequested();
        LastActivity = DateTime.Now.Ticks;
        var messageId = PacketReader.ReadInt32();
        var encrypted = PacketReader.ReadBoolean();
        var size = PacketReader.ReadInt32(); //TODO Check if message length is valid
        HandleReceivePacket(messageId, encrypted, size);
      }
    }
    catch (Exception ex) {
      //TODO Trigger event
    }
    finally {
      Disconnect(DisconnectReason.Unknown);
    }
  }

  private void Disconnect(DisconnectReason unknown) {
    
  }
}