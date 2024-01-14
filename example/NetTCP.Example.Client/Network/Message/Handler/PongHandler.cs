using System.Net.Sockets;
using NetTCP.Abstract;
using NetTCP.Attributes;
using NetTCP.Client;
using NetTCP.Example.Shared;
using NetTCP.Example.Shared.Network.Packets.Client;
using NetTCP.Example.Shared.Network.Packets.Server;

namespace NetTCP.Example.Client.Network.Message.Handler;

public class PongHandler : IPacketHandler
{
  [PacketHandler(OpCodes.SMPong)]
  public static void HandlePing(EasTcpClient client, SMPong request) {
    client.EnqueuePacketSend(new CMPing() {
      Timestamp = request.Timestamp
    });
  }
}