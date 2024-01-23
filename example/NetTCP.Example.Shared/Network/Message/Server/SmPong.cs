using NetTCP.Abstract;
using NetTCP.Attributes;

namespace NetTCP.Example.Shared.Network.Message.Server;

[Packet(OpCodes.SMPong)]
public sealed class SmPong : IPacket
{
}