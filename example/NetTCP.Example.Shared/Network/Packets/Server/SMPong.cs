using NetTCP.Abstract;
using NetTCP.Attributes;
using NetTCP.Serialization;

namespace NetTCP.Example.Shared.Network.Packets.Server;

[Packet(OpCodes.SMPong)]
public class SMPong : IWriteablePacket,
                      IReadablePacket
{
  public long Timestamp { get; set; } = 0;

  public void Write(PacketWriter writer) {
    writer.Write(Timestamp);
  }

  public void Read(PacketReader reader) {
    Timestamp = reader.ReadInt64();
  }
}