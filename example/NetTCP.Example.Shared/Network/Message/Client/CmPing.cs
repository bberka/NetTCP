using NetTCP.Abstract;
using NetTCP.Attributes;
using NetTCP.Network;

namespace NetTCP.Example.Shared.Network.Message.Client;

[Packet(OpCodes.CMPing)]
public sealed class CmPing : IPacket
{
  public long Ticks { get; set; } = DateTime.Now.Ticks;
  public  void Read(TcpPacketReader reader) {
    Ticks = reader.ReadLong();
  }

  public  void Write(TcpPacketWriter writer) {
    writer.Write(Ticks);
  }
}