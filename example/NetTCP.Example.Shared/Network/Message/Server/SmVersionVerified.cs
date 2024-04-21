using NetTCP.Abstract;
using NetTCP.Attributes;
using NetTCP.Network;

namespace NetTCP.Example.Shared.Network.Message.Server;

[Packet(OpCodes.SmVersionVerified,PacketType.Server)]
public class SmVersionVerified : IPacket
{

  public string Message { get; set; } = "VersionMismatch";
  public void Write(TcpPacketWriter writer) {
    writer.Write(Message);
  }

  public void Read(TcpPacketReader reader) {
    Message = reader.ReadString();
  }
}