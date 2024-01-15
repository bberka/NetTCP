using System.Net.Sockets;
using Autofac;
using NetTCP.Abstract;
using NetTCP.Attributes;
using NetTCP.Client;
using NetTCP.Example.Server.Network.Message.Client;
using NetTCP.Example.Server.Network.Message.Server;
using NetTCP.Example.Shared;
using NetTCP.Example.Shared.Network.Message.Client;
using NetTCP.Example.Shared.Network.Message.Server;

namespace NetTCP.Example.Client.Network.Message.Handler;

public static class PongHandler
{
  [PacketHandler(OpCodes.SMPong)]
  public static void HandlePing(NetTcpClient client, SmPong request, ILifetimeScope scope) {
    Console.WriteLine($"Ping received from {client.AnyPacketsProcessing} with timestamp {request.Timestamp}.");
    client.EnqueuePacketSend(new CmPing() {
      Timestamp = request.Timestamp
    });
  }
}