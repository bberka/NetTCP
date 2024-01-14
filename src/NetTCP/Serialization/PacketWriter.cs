using NetTCP.Abstract;

namespace NetTCP.Serialization;

public class PacketWriter
{
  private readonly BinaryWriter _binaryWriter;

  
  public PacketWriter(Stream output) {
    _binaryWriter = new BinaryWriter(output);
  }
  public PacketWriter(BinaryWriter binaryWriter) {
    _binaryWriter = binaryWriter;
  }

  public void Write(bool value) {
    _binaryWriter.Write(value);
  }

  public void Write(int value) {
    _binaryWriter.Write(value);
  }

  public void Write(ulong value) {
    _binaryWriter.Write(value);
  }

  public void Write(long value) {
    _binaryWriter.Write(value);
  }

  public void Write(uint value) {
    _binaryWriter.Write(value);
  }

  public void Write(short value) {
    _binaryWriter.Write(value);
  }

  public void Write(ushort value) {
    _binaryWriter.Write(value);
  }

  public void Write(string value) {
    var length = value.Length;
    _binaryWriter.Write(length);
    _binaryWriter.Write(value);
  }

  public void Write(char value) {
    _binaryWriter.Write(value);
  }

  public void Write(sbyte value) {
    _binaryWriter.Write(value);
  }

  public void Write(float value) {
    _binaryWriter.Write(value);
  }

  public void Write(decimal value) {
    _binaryWriter.Write(value);
  }

  public void Write(double value) {
    _binaryWriter.Write(value);
  }
  
   public void Write(byte[] value) {
    _binaryWriter.Write(value); 
  }

  public void Write(IPacketWriteable message) {
    message.Write(this);
  }
  
  public void Flush() {
    _binaryWriter.BaseStream.Flush();
    _binaryWriter.Flush();
  }
}

