using Autofac;
using NetTCP.Attributes;
using NetTCP.Client;
using NetTCP.Example.Shared;
using NetTCP.Example.Shared.Network.Message.Client;
using NetTCP.Example.Shared.Network.Message.Server;

namespace NetTCP.Example.Client.Network.Message.Handler;

public static class PongHandler
{
  [PacketHandler(OpCodes.SMPong)]
  public static void HandlePing(NetTcpClient client, SmPong request, ILifetimeScope scope) {
    Console.WriteLine($"[NetTCP - Client] Pong received from server with timestamp {request.Timestamp}.");
    Thread.Sleep(1000);//dont do this
    client.EnqueuePacketSend(new CmPing {
      Timestamp = request.Timestamp + 1
    });
  }
}