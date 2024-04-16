namespace NetTCP.Abstract;

public interface INetTcpEncryptionProvider
{
  public byte[] Encrypt(byte[] data);
  
  public byte[] Decrypt(byte[] data);
}