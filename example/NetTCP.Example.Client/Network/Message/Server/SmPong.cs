using NetTCP.Abstract;
using NetTCP.Serialization;
using NetTCP.Example.Shared.Network.Message.Server;

namespace NetTCP.Example.Server.Network.Message.Server;

public sealed class SmPong : BaseSmPong,
                             IReadablePacket
{

  public void Read(PacketReader reader) {
    Timestamp = reader.ReadInt64();
  }
}