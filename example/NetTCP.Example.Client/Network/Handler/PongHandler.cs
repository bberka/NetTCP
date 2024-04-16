using Autofac;
using NetTCP.Attributes;
using NetTCP.Client;
using NetTCP.Example.Shared;
using NetTCP.Example.Shared.Network.Message.Client;
using NetTCP.Example.Shared.Network.Message.Server;

namespace NetTCP.Example.Client.Network.Message.Handler;

public static class PongHandler
{
  public static void HandlePing(NetTcpClient client, SmPong request) {
    client.EnqueuePacketSend(new CmPing() {
      Ticks = (DateTime.Now - client.ConnectedAtUtc).Ticks
    });
    Console.WriteLine($"Received tick: {request.Ticks}");
  }
}