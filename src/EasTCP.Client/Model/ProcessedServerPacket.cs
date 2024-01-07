using EasTCP.Abstract;

namespace EasTCP.Client.Model;

public class ProcessedServerPacket : PacketBase
{
  public ProcessedServerPacket(int messageId,
                          IPacketReadable message,
                          bool encrypted) {
    MessageId = messageId;
    Encrypted = encrypted;
    Message = message;
  }
  public IPacketReadable Message { get; set; }

}