namespace EasTCP.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class PacketAttribute : Attribute
{
  public int MessageId { get; }

  public bool Encrypted { get; set; } = false;
  public PacketAttribute(int messageId) {
    MessageId = messageId;
  }

  public PacketAttribute(object @enum) {
    var isTypeEnum = @enum.GetType().IsEnum;
    if (!isTypeEnum) throw new ArgumentException("Must be an enum type", nameof(@enum));
    var asInt = Convert.ToInt32(@enum);
    MessageId = asInt;
  }
}