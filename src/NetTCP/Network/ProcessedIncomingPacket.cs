using NetTCP.Abstract;

namespace NetTCP.Network;

public class ProcessedIncomingPacket : PacketBase
{
  public ProcessedIncomingPacket(int messageId,
                                 bool encrypted,
                                 IPacket message
  ) {
    MessageId = messageId;
    Encrypted = encrypted;
    Message = message;
  }

  public IPacket Message { get; set; }
}