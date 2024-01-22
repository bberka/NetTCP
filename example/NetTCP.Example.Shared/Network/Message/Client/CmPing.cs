using NetTCP.Abstract;
using NetTCP.Attributes;

namespace NetTCP.Example.Shared.Network.Message.Client;

[Packet(OpCodes.CMPing)]
public sealed class CmPing : IPacket
{
  public long Timestamp { get; set; } = 0;
}