using NetTCP.Abstract;

namespace NetTCP.Server.Model;

public class ProcessedClientPacket : PacketBase
{
  public ProcessedClientPacket(int messageId,
                          IPacketReadable message,
                          bool encrypted) {
    MessageId = messageId;
    Encrypted = encrypted;
    Message = message;
  }
  public IPacketReadable Message { get; set; }

}