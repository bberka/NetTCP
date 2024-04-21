using Autofac;
using NetTCP.Attributes;
using NetTCP.Example.Server.Abstract;
using NetTCP.Example.Shared;
using NetTCP.Example.Shared.Network.Message.Common;
using NetTCP.Server;

namespace NetTCP.Example.Server.Network.Message.Handler;

public static class MessageHandlers
{
  public static void Handle(NetTcpConnection connection, CmPing request) {
    var serverInfoMgr = connection.Scope.Resolve<IServerInfoMgr>();
    connection.EnqueuePacketSend(new SmPong {
      Ticks = (DateTime.Now - connection.ConnectedAtUtc).Ticks
    });
    Console.WriteLine($"Received tick: {request.Ticks}");
  }

  public static void Handle(NetTcpConnection connection, VersionInformation request) {
    var version = connection.Scope.Resolve<IServerInfoMgr>().Version;
    if (request.Version != version) {
      Console.WriteLine($"Version mismatch, expected: {version}, received: {request.Version}");
      connection.EnqueuePacketSend(new SmVersionMismatch() {
        Message = $"Server version mismatch, expected: {version}, received: {request.Version}"
      });
      connection.Disconnect(NetTcpErrorReason.VersionMismatch);
    }
    else {
      Console.WriteLine($"Version verified: {version}");
      connection.EnqueuePacketSend(new SmVersionVerified() {
        Message = $"Server version verified: {version}"
      });
    }
  }
  
}