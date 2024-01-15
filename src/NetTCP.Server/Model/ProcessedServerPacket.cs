using NetTCP.Abstract;
using NetTCP.Serialization;

namespace NetTCP.Server.Model;

public class ProcessedServerPacket  : PacketBase
{
  public ProcessedServerPacket(int messageId, bool encrypted, IWriteablePacket message) {
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