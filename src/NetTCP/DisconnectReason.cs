namespace NetTCP;

public enum DisconnectReason : byte
{
  Unknown,
  Timeout,
  ServerStopped,
  InvalidPacket,
  ClientDisconnected,
  InvalidOperation,
  PacketTransmissionError
}