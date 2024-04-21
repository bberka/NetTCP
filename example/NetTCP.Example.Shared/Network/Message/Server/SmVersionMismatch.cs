﻿using NetTCP.Abstract;
using NetTCP.Attributes;
using NetTCP.Network;

namespace NetTCP.Example.Shared.Network.Message.Server;

[Packet(OpCodes.SmVersionMismatch,PacketType.Server)]
public class SmVersionMismatch : IPacket
{
  public string Message { get; set; } = "VersionMismatch";
  public void Write(TcpPacketWriter writer) {
    writer.Write(Message);
  }

  public void Read(TcpPacketReader reader) {
    Message = reader.ReadString();
  }
}