using NetTCP.Abstract;
using NetTCP.Serialization;

namespace NetTCP.Client.Model;

public class ProcessedClientPacket  : PacketBase
{
  public ProcessedClientPacket(int messageId, IWriteablePacket message, bool encrypted) {
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