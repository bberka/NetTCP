using NetTCP.Abstract;
using NetTCP.Example.Shared.Network.Message.Client;
using NetTCP.Serialization;

namespace NetTCP.Example.Server.Network.Message.Client;

public sealed class CmPing : BaseCmPing,
                             IWriteablePacket
{
  public void Write(PacketWriter writer) {
    writer.Write(Timestamp);
  }
}