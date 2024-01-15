using NetTCP.Abstract;

namespace NetTCP.Server.Model;

public class ProcessedClientPacket : PacketBase
{
  public ProcessedClientPacket(int messageId,
                          bool encrypted,
                          IPacketReadable message
    ) {
    MessageId = messageId;
    Encrypted = encrypted;
    Message = message;
  }
  public IPacketReadable Message { get; set; }

}