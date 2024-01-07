using EasTCP.Serialization;

namespace EasTCP.Abstract;

public interface IPacketReadable
{
  public abstract void Read(PacketReader reader);
  
}