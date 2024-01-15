using NetTCP.Serialization;

namespace NetTCP.Abstract;

public interface IWriteablePacket
{
  public abstract void Write(PacketWriter writer);

}