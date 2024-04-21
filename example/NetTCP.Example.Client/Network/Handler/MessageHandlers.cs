using Autofac;
using NetTCP.Attributes;
using NetTCP.Client;
using NetTCP.Example.Shared;
using NetTCP.Example.Shared.Network.Message.Client;
using NetTCP.Example.Shared.Network.Message.Common;
using NetTCP.Example.Shared.Network.Message.Server;

namespace NetTCP.Example.Client.Network.Message.Handler;

public static class MessageHandlers
{
  public static void Handle(NetTcpClient client, SmPong request) {
    client.EnqueuePacketSend(new CmPing() {
      Ticks = (DateTime.Now - client.ConnectedAtUtc).Ticks
    });
    Console.WriteLine($"Received tick: {request.Ticks}");
  }

  public static void Handle(NetTcpClient client, VersionInformation request) {
    Console.WriteLine($"Received version: {request.Version}");
  }

  public static void Handle(NetTcpClient client, SmVersionMismatch request) {
    Console.WriteLine($"Version mismatch: {request.Message}");
  }

  public static void Handle(NetTcpClient client, SmVersionVerified request) {
    Console.WriteLine($"Version verified: {request.Message}");
  }
}