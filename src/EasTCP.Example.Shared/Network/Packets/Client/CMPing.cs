using EasTCP.Abstract;
using EasTCP.Attributes;
using EasTCP.Serialization;

namespace EasTCP.Example.Shared.Network.Packets.Client;

[Packet(OpCodes.CMPing)]
public class CMPing : IPacketWriteable,
                      IPacketReadable
{
  public long Timestamp { get; set; } = 0;

  public void Write(PacketWriter writer) {
    writer.Write(Timestamp);
  }

  public void Read(PacketReader reader) {
    Timestamp = reader.ReadInt64();
  }
}