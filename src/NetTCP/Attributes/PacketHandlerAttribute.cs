namespace NetTCP.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class PacketHandlerAttribute : Attribute
{
  public PacketHandlerAttribute(int messageId) {
    MessageId = messageId;
  }

  public PacketHandlerAttribute(object @enum) {
    var isTypeEnum = @enum.GetType().IsEnum;
    if (!isTypeEnum) throw new ArgumentException("Must be an enum type", nameof(@enum));
    var asInt = Convert.ToInt32(@enum);
    MessageId = asInt;
  }

  public int MessageId { get; }
}