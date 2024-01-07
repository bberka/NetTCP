using System.Net.Sockets;
using EasTCP.Abstract;
using EasTCP.Attributes;
using EasTCP.Client;
using EasTCP.Example.Shared;
using EasTCP.Example.Shared.Network.Packets.Client;
using EasTCP.Example.Shared.Network.Packets.Server;

namespace EasTCP.Example.Client.Network.Message.Handler;

public class PongHandler : IPacketHandler
{
  [PacketHandler(OpCodes.SMPong)]
  public static void HandlePing(EasTcpClient client, SMPong request) {
    client.EnqueuePacketSend(new CMPing() {
      Timestamp = request.Timestamp
    });
  }
}