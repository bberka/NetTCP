using NetTCP.Abstract;

namespace NetTCP.Server.Model;

public class ProcessedClientPacket : PacketBase
{
  public ProcessedClientPacket(int messageId,
                          bool encrypted,
                          IReadablePacket message
    ) {
    MessageId = messageId;
    Encrypted = encrypted;
    Message = message;
  }
  public IReadablePacket Message { get; set; }

}