using System.Net.Sockets;
using Autofac;

namespace NetTCP.Abstract;

public interface INetTcpSession
{
  string RemoteIpAddress { get; }
  ushort RemotePort { get; }
  bool CanRead { get; }
  bool AnyIncomingPackets { get; }
  bool AnyOutgoingPackets { get; }
  bool RunningHandler { get; }
  DateTime ConnectedAtUtc { get; }
  Guid ConnectionId { get; }

  /// <summary>
  /// Autofac scope for the connection, lasts as long as the connection is alive.
  /// </summary>
  ILifetimeScope Scope { get; }

  TcpClient Client { get; }
  long LastActivity { get; }

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
  int ProcessingWaitTimeoutSecondsOnDisconnect { get; set; }

  /// <summary>
  /// Disconnects and disposes the client. This method will not trigger any events.
  /// </summary>
  void Dispose();

  void Disconnect(NetTcpErrorReason netTcpErrorReason = NetTcpErrorReason.Unknown);
  
  void EnqueuePacketSend(IPacket message, bool encrypted = false);
  
}