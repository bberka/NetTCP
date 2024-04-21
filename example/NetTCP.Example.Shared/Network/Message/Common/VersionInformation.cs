using NetTCP.Abstract;
using NetTCP.Attributes;
using NetTCP.Network;

namespace NetTCP.Example.Shared.Network.Message.Common;

[Packet(OpCodes.VersionInformation,PacketType.ClientAndServer)]
public class VersionInformation : IPacket
{
  public string Version { get; set; } = "1.0.0";
  public bool Debug { get; set; } = true;
  public bool Beta { get; set; } = false;

  public void Write(TcpPacketWriter writer) {
    writer.Write(Version);
    writer.Write(Debug);
    writer.Write(Beta);
  }

  public void Read(TcpPacketReader reader) {
    Version = reader.ReadString();
    Debug = reader.ReadBool();
    Beta = reader.ReadBool();
  }
}