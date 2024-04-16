using NetTCP.Abstract;

namespace NetTCP.Network;

public class ProcessedOutgoingPacket : ProcessedPacketBase
{
  public ProcessedOutgoingPacket(int messageId, bool encrypted, byte[] body) {
    MessageId = messageId;
    Encrypted = encrypted;
    Body = body;
  }
  public int Size => Body.Length;
  public byte[] Body { get; }
}