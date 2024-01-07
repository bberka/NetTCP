using EasTCP.Serialization;

namespace EasTCP.Abstract;

public interface IPacketWriteable
{
  public abstract void Write(PacketWriter writer);

}