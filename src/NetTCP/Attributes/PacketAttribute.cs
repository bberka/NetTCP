namespace NetTCP.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class PacketAttribute : Attribute
{
  public PacketAttribute(int messageId,PacketType type = PacketType.ClientAndServer) {
    Type = type;
    MessageId = messageId;
  }

  public PacketAttribute(object @enum, PacketType type = PacketType.ClientAndServer) {
    Type = type;
    var isTypeEnum = @enum.GetType().IsEnum;
    if (!isTypeEnum) throw new ArgumentException("Must be an enum type", nameof(@enum));
    var asInt = Convert.ToInt32(@enum);
    MessageId = asInt;
  }

  public int MessageId { get; }
  public PacketType Type { get; }

  public bool Encrypted { get; set; } = false;
}