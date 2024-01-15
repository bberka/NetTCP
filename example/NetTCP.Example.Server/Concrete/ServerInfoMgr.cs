using NetTCP.Example.Server.Abstract;

namespace NetTCP.Example.Server.Concrete;

public class ServerInfoMgr : IServerInfoMgr
{
  public ServerInfoMgr() {
    Name = "NetTCP Example Server";
  }

  public string Name { get; set; }
}