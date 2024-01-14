using NetTCP.Serialization;

namespace NetTCP.Abstract;

public interface IPacketReadable
{
  public abstract void Read(PacketReader reader);
  
}