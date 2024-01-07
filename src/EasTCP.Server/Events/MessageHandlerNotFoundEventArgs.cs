﻿using EasTCP.Server.Model;

namespace EasTCP.Server.Events;

public class MessageHandlerNotFoundEventArgs
{
  public EasTcpConnection Connection { get; }
  public ProcessedClientPacket Packet { get; }

  public MessageHandlerNotFoundEventArgs(EasTcpConnection connection, ProcessedClientPacket packet) {
    Connection = connection;
    Packet = packet;
  }


  

}