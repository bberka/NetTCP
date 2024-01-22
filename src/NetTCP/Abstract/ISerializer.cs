namespace NetTCP.Abstract;

/// <summary>
/// A base serializer interface that will be used to serialize and deserialize the packets.
/// By default it will use the <see cref="JsonSerializer"/> but you can create your own serializer.
/// It is recommended to make a custom serializer when performance is crucial.
/// You can register your custom serializer by using the <see cref="NetTCPClientBuilder.UseSerializer"/> or <see cref="NetTCPServerBuilder.UseSerializer"/> method.
/// Be sure to register same serializer on both client and server.
/// </summary>
public interface ISerializer
{
  public byte[] Serialize<T>(T obj) where T : IPacket;
  
  /// <summary>
  /// This method is used to deserialize the bytes into the packet instance.
  /// It will return the packet instance with the data from the bytes.
  /// </summary>
  /// <param name="packetInstance"></param>
  /// <param name="bytes"></param>
  /// <returns></returns>
  public IPacket Deserialize(IPacket packetInstance, byte[] bytes);
}