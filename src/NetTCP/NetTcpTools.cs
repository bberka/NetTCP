namespace NetTCP;

public static class NetTcpTools
{
  public const int MIN_PORT_VALUE = 0;
  public const int MAX_PORT_VALUE = 65535;

  public static bool IsValidPort(ushort port) {
    return port > 0 && port < 65535;
  }
}