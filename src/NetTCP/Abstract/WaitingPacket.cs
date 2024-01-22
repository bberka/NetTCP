namespace NetTCP.Abstract;

public class WaitingPacket
{
  public int MessageId { get; set; }
  public bool Encrypted { get; set; }

  /// <summary>
  ///   Size of the body byte array
  /// </summary>
  public int Size { get; set; }

  public byte[] Body { get; set; }
}