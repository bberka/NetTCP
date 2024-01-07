using EasTCP.Abstract;
using EasTCP.Serialization;

namespace EasTCP.Server.Model;

public class ProcessedServerPacket  : PacketBase
{
  public ProcessedServerPacket(int messageId, IPacketWriteable message, bool encrypted) {
    var memoryStream = new MemoryStream();
    var writer = new PacketWriter(memoryStream);
    message.Write(writer);
    Data = memoryStream.ToArray();
    MessageId = messageId;
    Encrypted = encrypted;
    Size = (int)Data.Length;
  }
  
  public int Size { get; protected set; }
  
  public byte[] Data { get; protected set; }
  
}