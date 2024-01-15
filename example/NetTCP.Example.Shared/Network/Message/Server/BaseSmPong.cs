using NetTCP.Attributes;

namespace NetTCP.Example.Shared.Network.Message.Server;

[Packet(OpCodes.SMPong)]
public abstract class BaseSmPong 
{
  public long Timestamp { get; set; } = 0;

}