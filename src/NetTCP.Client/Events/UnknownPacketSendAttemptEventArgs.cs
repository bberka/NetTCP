using NetTCP.Abstract;

namespace NetTCP.Client.Events;

public sealed class UnknownPacketSendAttemptEventArgs
{
  internal UnknownPacketSendAttemptEventArgs(NetTcpClient client, IPacket message, bool encrypted) {
    Client = client;
    Message = message;
    Encrypted = encrypted;
  }

  public NetTcpClient Client { get; }
  public IPacket Message { get; }
  public bool Encrypted { get; }
}