namespace NetTCP.Abstract;

public abstract class PacketBase
{
  public int MessageId { get; protected set; }

  public bool Encrypted { get; protected set; }
}