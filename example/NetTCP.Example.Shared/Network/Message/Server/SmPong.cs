using NetTCP.Abstract;
using NetTCP.Attributes;
using NetTCP.Network;

namespace NetTCP.Example.Shared.Network.Message.Server;

[Packet(OpCodes.SMPong)]
public sealed class SmPong : IPacket
{
  public long Ticks { get; set; }

  public void Read(TcpPacketReader reader) {
    Ticks = reader.ReadLong();
  }

  public void Write(TcpPacketWriter writer) {
    writer.Write(Ticks);
  }
}