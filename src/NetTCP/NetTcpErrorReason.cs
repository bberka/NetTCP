﻿  namespace NetTCP;

public enum NetTcpErrorReason : byte
{
  Unknown,
  Timeout,
  ConnectionClosed,
  ServerStopped,
  InvalidPacket,
  ClientDisconnected,
  InvalidOperation,
  PacketTransmissionError,
  PacketSendQueueError,
  PacketInvokeHandlerError,
  PacketReceiveQueueError,
  PacketReceiveHandleError,
  NetworkStreamReadError,
  ConnectionFailed,
  ClientDisposed,
  CanNotProcess,
  EncryptionProviderNotFound,
  VersionMismatch
}