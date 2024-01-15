using NetTCP.Serialization;

namespace NetTCP.Abstract;

public interface IReadablePacket
{
  public abstract void Read(PacketReader reader);
  
}