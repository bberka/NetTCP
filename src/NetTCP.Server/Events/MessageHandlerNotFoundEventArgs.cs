using NetTCP.Server.Model;

namespace NetTCP.Server.Events;

public class MessageHandlerNotFoundEventArgs
{
  public NetTcpConnection Connection { get; }
  public ProcessedClientPacket Packet { get; }

  public MessageHandlerNotFoundEventArgs(NetTcpConnection connection, ProcessedClientPacket packet) {
    Connection = connection;
    Packet = packet;
  }


  

}