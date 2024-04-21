namespace NetTCP.Example.Shared;

public enum OpCodes
{
  CmPing = 1000,
  SmPong,
  SmVersionMismatch,
  SmVersionVerified,
  VersionInformation
}