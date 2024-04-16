using NetTCP.Network;

namespace NetTCP.Abstract;

public interface IPacket : IWriteable,
                           IReadable { }

public interface IReadable
{
  public void Read(TcpPacketReader reader);
}

public interface IWriteable
{
  public void Write(TcpPacketWriter writer);
}