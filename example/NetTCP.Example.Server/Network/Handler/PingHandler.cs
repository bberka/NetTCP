using Autofac;
using NetTCP.Attributes;
using NetTCP.Example.Server.Abstract;
using NetTCP.Example.Shared;
using NetTCP.Server;

namespace NetTCP.Example.Server.Network.Message.Handler;

public static class PingHandler
{
  public static void HandlePing(NetTcpConnection connection, CmPing request, ILifetimeScope scope) {
    var serverInfoMgr = scope.Resolve<IServerInfoMgr>();
    Console.WriteLine($"[NetTCP - Server - {serverInfoMgr.Name}] Ping received from {connection.RemoteIpAddress} ");
    Thread.Sleep(1000); //dont do this
    connection.EnqueuePacketSend(new SmPong {});
  }
}