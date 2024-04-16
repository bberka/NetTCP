using NetTCP.Abstract;

namespace NetTCP.Network;

public class ProcessedIncomingPacket : ProcessedPacketBase
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