using NetTCP.Server.Model;

namespace NetTCP.Server.Events;

public class MessageHandlerNotFoundEventArgs
{
  public EasTcpConnection Connection { get; }
  public ProcessedClientPacket Packet { get; }

  public MessageHandlerNotFoundEventArgs(EasTcpConnection connection, ProcessedClientPacket packet) {
    Connection = connection;
    Packet = packet;
  }


  

}