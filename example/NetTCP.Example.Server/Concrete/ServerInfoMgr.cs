using NetTCP.Example.Server.Abstract;

namespace NetTCP.Example.Server.Concrete;

public class ServerInfoMgr : IServerInfoMgr
{
  public ServerInfoMgr() {
    Name = "NetTCP Example Server";
    Version = "1.0.0";
  }

  public string Name { get; set; }
  public string Version { get; set; }
}