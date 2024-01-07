using System.Collections.Concurrent;
using System.Text;
using EasTCP.Abstract;
using EasTCP.Client.Model;
using EasTCP.Serialization;

namespace EasTCP.Client;

public class EasTcpClient
{

  public System.Net.Sockets.TcpClient Client { get; set; }
  private ConcurrentQueue<ProcessedClientPacket> _outgoingPacketQueue;
  private ConcurrentQueue<ProcessedServerPacket> _incomingPacketQueue;

  private PacketReader _packetReader;
  private PacketWriter _packetWriter;
  protected CancellationToken ServerCancellationToken { get; }
  protected CancellationTokenSource ClientCancellationTokenSource { get; }

  public bool CanProcess => Client.Connected && !ClientCancellationTokenSource.IsCancellationRequested;

  public bool AnyProcessingPackets => !_incomingPacketQueue.IsEmpty || _outgoingPacketQueue.IsEmpty;


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

  

  public EasTcpClient(string host, ushort port) {
    Client = new System.Net.Sockets.TcpClient(host, port);
    ClientCancellationTokenSource = new CancellationTokenSource();
    _outgoingPacketQueue = new ConcurrentQueue<ProcessedClientPacket>();
    _incomingPacketQueue = new ConcurrentQueue<ProcessedServerPacket>();


  }

  public void Connect() {
    StartHandleConnectionTask();
    Task.Run(HandleOutgoingPacketQueue,ClientCancellationTokenSource.Token);
    Task.Run(HandleIncomingPacketQueue,ClientCancellationTokenSource.Token);
    
     
     
  }

  private async Task HandleOutgoingPacketQueue() {
    try {
      while (CanProcess) {
        if (_outgoingPacketQueue.TryDequeue(out var packet) && packet != null) {
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

  private async Task HandleIncomingPacketQueue() {
    try {
      while (CanProcess) {
        if (_incomingPacketQueue.TryDequeue(out var packet) && packet != null) {
          var messageHandler = ClientPacketTable.This.GetMessageHandler(packet.MessageId);
          if (messageHandler == null) {
            //invalid message handler
            //TODO Trigger event
            return;
          }

          try {
            messageHandler.Invoke(this, packet.Message);
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

  public void EnqueuePacketSend(IPacketWriteable message,
                                bool encrypted = false) {
    if (!ClientPacketTable.This.GetOpcode(message, out var opcode)) {
      //invalid message opcode
      //TODO Trigger event
      return;
    }
    var serverPacket = new ProcessedClientPacket(opcode, message, encrypted);
    _outgoingPacketQueue.Enqueue(serverPacket);
  }


  private void HandleReceivePacket(int messageId, bool encrypted, int size) {
    Task.Run((() => {
                 var messageInstance = ClientPacketTable.This.GetMessage(messageId);
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

  private async void StartHandleConnectionTask() {
    Task.Run(() => {
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
    },ClientCancellationTokenSource.Token);
  }

  private void Disconnect(DisconnectReason unknown) {
    
  }
}