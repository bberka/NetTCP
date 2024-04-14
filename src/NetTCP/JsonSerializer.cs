using System.Collections.Immutable;
using System.Text;
using NetTCP.Abstract;
using Newtonsoft.Json;

namespace NetTCP;

public class JsonSerializer : ISerializer
{
  public byte[] Serialize(IPacket obj) {
    var str = JsonConvert.SerializeObject(obj);
    return Encoding.UTF8.GetBytes(str);
  }

  public IPacket Deserialize(IPacket packetInstance, byte[] bytes) {
    var str = Encoding.UTF8.GetString(bytes);
    var type = packetInstance.GetType();
    return (IPacket)JsonConvert.DeserializeObject(str, type);
  }
}