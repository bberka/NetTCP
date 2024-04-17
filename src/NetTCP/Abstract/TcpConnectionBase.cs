using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using Autofac;
using NetTCP.Network;

namespace NetTCP.Abstract;

public abstract class NetTcpConnectionBase : IDisposable,
                                             INetTcpSession

{
  public NetTcpConnectionBase(Guid connectionId) {
    ConnectionId = connectionId;
    CancellationTokenSource = new CancellationTokenSource();
    IncomingPacketQueue = new ConcurrentQueue<ProcessedIncomingPacket>();
    OutgoingPacketQueue = new ConcurrentQueue<ProcessedOutgoingPacket>();
    ConnectedAtUtc = DateTime.UtcNow;
  }

  public NetTcpConnectionBase() {
    ConnectionId = Guid.NewGuid();
    CancellationTokenSource = new CancellationTokenSource();
    IncomingPacketQueue = new ConcurrentQueue<ProcessedIncomingPacket>();
    OutgoingPacketQueue = new ConcurrentQueue<ProcessedOutgoingPacket>();
    ConnectedAtUtc = DateTime.UtcNow;
  }

  public string RemoteIpAddress { get; protected set; }
  public ushort RemotePort { get; protected set; }

  public bool CanRead {
    get {
      if (Client == null) return false;
      if (Client.Client == null) return false;
      if (!Client.Connected) return false;
      if (CancellationTokenSource.IsCancellationRequested) return false;
      try {
        if (NetworkStream == null) return false;
        if (!NetworkStream.CanRead) return false;
        if (!NetworkStream.CanWrite) return false;
      }
      catch (Exception) {
        //Ignored
      }

      return true;
    }
  }

  public bool AnyIncomingPackets => !IncomingPacketQueue.IsEmpty;

  public bool AnyOutgoingPackets => !OutgoingPacketQueue.IsEmpty;

  public bool RunningHandler { get; protected set; }
  public DateTime ConnectedAtUtc { get; protected set; }
  public Guid ConnectionId { get; protected set; }

  /// <summary>
  /// Autofac scope for the connection, lasts as long as the connection is alive.
  /// </summary>
  public ILifetimeScope Scope { get; protected set; }

  public TcpClient Client { get; protected set; }

  protected ConcurrentQueue<ProcessedIncomingPacket> IncomingPacketQueue { get; set; } = new();
  protected ConcurrentQueue<ProcessedOutgoingPacket> OutgoingPacketQueue { get; set; } = new();

  protected CancellationTokenSource CancellationTokenSource { get; set; }

  public long LastActivity { get; protected set; }

  protected internal NetworkStream NetworkStream => Client.GetStream();


  private object _disposeLock = new();

  private object _disconnectLock = new();

  /// <summary>
  /// Disconnects and disposes the client. This method will not trigger any events.
  /// </summary>
  public void Dispose() {
    lock (_disposeLock) {
      if (CancellationTokenSource is not null) {
        CancellationTokenSource?.Dispose();
      }

      if (Scope is not null) {
        Scope?.Dispose();
      }

      if (Client is not null) {
        Client.Dispose();
      }
    }
  }

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

  protected void ProcessOutgoingPacket(int opcode, bool encrypted, IPacket message) {
    var writer = new TcpPacketWriter();
    message.Write(writer);
    var bytes = writer.ToArray();
    if (encrypted) {
      var providerExists = Scope.TryResolve<INetTcpEncryptionProvider>(out var provider);
      if (!providerExists) {
        throw new Exception("Encryption provider not found, packet impossible to send, please register an encryption provider in the service container. OpCode: " + opcode);
      }

      bytes = provider.Encrypt(bytes);
    }
    
    var packet = new ProcessedOutgoingPacket(opcode, encrypted, bytes);
    OutgoingPacketQueue.Enqueue(packet);
  }


  /// <summary>
  ///  Closes client connection, waits for all incoming packets to be processed and disposes the connection and other resources.
  /// </summary>
  /// <param name="netTcpErrorReason"></param>
  protected void ProcessDisconnect(NetTcpErrorReason netTcpErrorReason) {
    lock (_disconnectLock) {
      Client.Close();
      var timer = new Stopwatch();
      while (AnyIncomingPackets || RunningHandler) {
        if (ProcessingWaitTimeoutSecondsOnDisconnect == 0) {
          break;
        }

        if (ProcessingWaitTimeoutSecondsOnDisconnect > 0) {
          var isTimeout = timer.Elapsed.TotalSeconds > ProcessingWaitTimeoutSecondsOnDisconnect;
          if (isTimeout) {
            Debug.WriteLine("Timeout waiting for incoming packets to be processed", "NetTcpConnectionBase");
            break;
          }
        }

        Task.Delay(100).GetAwaiter().GetResult();
      }
      Dispose();
    }
  }


  /// <summary>
  /// The time in seconds to wait for incoming packets to be processed before disposing the client. Default is 30.
  /// <br/><br/>
  /// This will be used for waiting incoming packet processing timeout.
  /// <br/><br/>
  /// Once a connection ends server or client will wait for any processed incoming packet handlers to finish.
  /// <br/><br/>
  /// Set 0 to disable waiting for incoming packets to be processed.
  /// <br/>
  /// Set a negative value to wait indefinitely.
  /// <br/>
  /// It is best to change this value on a thread-safe manner.
  /// </summary>
  public int ProcessingWaitTimeoutSecondsOnDisconnect { get; set; } = 30;


  protected abstract void HandleOutgoingPackets();
  protected abstract void HandleIncomingPackets();
  protected abstract void HandleStream();
  public abstract void Disconnect(NetTcpErrorReason netTcpErrorReason = NetTcpErrorReason.Unknown);
  public abstract void EnqueuePacketSend(IPacket message, bool encrypted = false);
}