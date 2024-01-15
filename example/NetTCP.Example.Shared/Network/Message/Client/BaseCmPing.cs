using NetTCP.Attributes;

namespace NetTCP.Example.Shared.Network.Message.Client;

[Packet(OpCodes.CMPing)]
public abstract class BaseCmPing
{
  public long Timestamp { get;  set; } = 0;

}