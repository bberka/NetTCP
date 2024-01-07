using EasTCP.Abstract;
using EasTCP.Attributes;
using EasTCP.Example.Shared;
using EasTCP.Example.Shared.Network.Packets.Client;
using EasTCP.Example.Shared.Network.Packets.Server;
using EasTCP.Server;

namespace EasTCP.Example.Server.Network.Message.Handler;

public class PingHandler : IPacketHandler
{
  [PacketHandler(OpCodes.CMPing)]
  public static void HandlePing(EasTcpConnection connection, CMPing request) {
    connection.EnqueuePacketSend(new SMPong() {
      Timestamp = request.Timestamp
    });
  }
}