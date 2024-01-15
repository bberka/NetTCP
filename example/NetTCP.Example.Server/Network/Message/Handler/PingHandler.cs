using Autofac;
using Autofac.Core;
using NetTCP.Abstract;
using NetTCP.Attributes;
using NetTCP.Example.Server.Abstract;
using NetTCP.Example.Shared;
using NetTCP.Example.Shared.Network.Packets.Client;
using NetTCP.Example.Shared.Network.Packets.Server;
using NetTCP.Server;

namespace NetTCP.Example.Server.Network.Message.Handler;

public static class PingHandler
{
  [PacketHandler(OpCodes.CMPing)]
  public static void HandlePing(NetTcpConnection connection, CMPing request, ILifetimeScope scope) {
    var serverInfoMgr = scope.Resolve<IServerInfoMgr>();

    Console.WriteLine($"[{serverInfoMgr.Name}] Ping received from {connection.RemoteIpAddress} with timestamp {request.Timestamp}.");
    connection.EnqueuePacketSend(new SMPong() {
      Timestamp = request.Timestamp
    });
  }
}