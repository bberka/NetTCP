namespace NetTCP.Network;

public class ProcessedOutgoingPacket
{
  public ProcessedOutgoingPacket(int messageId, bool encrypted, byte[] body) {
    MessageId = messageId;
    Encrypted = encrypted;
    Body = body;
  }

  public int MessageId { get; }
  public bool Encrypted { get; }
  public int Size => Body.Length;
  public byte[] Body { get; }
}