using NetTCP.Abstract;
using NetTCP.Serialization;
using NetTCP.Example.Shared.Network.Message.Server;

namespace NetTCP.Example.Server.Network.Message.Server;

public sealed class SmPong : BaseSmPong,
                             IWriteablePacket
{
  public void Write(PacketWriter writer) {
    writer.Write(Timestamp);
  }
}