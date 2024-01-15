using NetTCP.Abstract;

namespace NetTCP.Client.Model;

public class ProcessedServerPacket : PacketBase
{
  public ProcessedServerPacket(int messageId,
                          IReadablePacket message,
                          bool encrypted) {
    MessageId = messageId;
    Encrypted = encrypted;
    Message = message;
  }
  public IReadablePacket Message { get; set; }

}