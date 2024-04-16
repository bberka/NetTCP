using Autofac;
using NetTCP.Attributes;
using NetTCP.Example.Server.Abstract;
using NetTCP.Example.Shared;
using NetTCP.Server;

namespace NetTCP.Example.Server.Network.Message.Handler;

public static class PingHandler
{
  public static void HandlePing(NetTcpConnection connection, CmPing request) {
    var serverInfoMgr = connection.Scope.Resolve<IServerInfoMgr>();
    connection.EnqueuePacketSend(new SmPong {
      Ticks = (DateTime.Now - connection.ConnectedAtUtc).Ticks
    });
    Console.WriteLine($"Received tick: {request.Ticks}");
  }
}