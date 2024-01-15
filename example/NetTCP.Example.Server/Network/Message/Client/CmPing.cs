
using NetTCP.Serialization;

namespace NetTCP.Example.Server.Network.Message.Client;

public sealed class CmPing : BaseCmPing, IReadablePacket
{
  public void Read(PacketReader reader) {
    Timestamp = reader.ReadInt64();
  }
}