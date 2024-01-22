using Autofac;
using NetTCP.Attributes;
using NetTCP.Example.Server.Abstract;
using NetTCP.Example.Shared;
using NetTCP.Server;

namespace NetTCP.Example.Server.Network.Message.Handler;

public static class PingHandler
{
  [PacketHandler(OpCodes.CMPing)]
  public static void HandlePing(NetTcpConnection connection, CmPing request, ILifetimeScope scope) {
    var serverInfoMgr = scope.Resolve<IServerInfoMgr>();
    Console.WriteLine($"[NetTCP - Server - {serverInfoMgr.Name}] Ping received from {connection.RemoteIpAddress} with timestamp {request.Timestamp}.");
    Thread.Sleep(1000);
    connection.EnqueuePacketSend(new SmPong {
      Timestamp = request.Timestamp + 1
    });
  }
}