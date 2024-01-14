using NetTCP.Serialization;

namespace NetTCP.Abstract;

public interface IPacketWriteable
{
  public abstract void Write(PacketWriter writer);

}