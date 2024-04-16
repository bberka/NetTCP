namespace NetTCP.Abstract;

public abstract class ProcessedPacketBase
{
  public int MessageId { get; protected set; }
  public bool Encrypted { get; protected set; }
}