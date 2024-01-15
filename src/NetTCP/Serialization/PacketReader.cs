using System.Text;

namespace NetTCP.Serialization;

public class PacketReader
{
  private readonly BinaryReader _binaryReader;
  // private readonly int _length;
  // private int _readLength;


  public PacketReader(Stream input) {
    _binaryReader = new BinaryReader(input);
    // _length = length;
  }

  public PacketReader(BinaryReader binaryReader) {
    _binaryReader = binaryReader;
    // _length = length;
  }


  private void CheckNextReadLengthOverflow(int length) {
    // if (_readLength + length > _length) {
    //   throw new Exception("Read overflow");
    // }
    // _readLength += length;
  }

  public long ReadInt64() {
    CheckNextReadLengthOverflow(8);
    return _binaryReader.ReadInt64();
  }

  public int ReadInt32() {
    CheckNextReadLengthOverflow(4);
    return _binaryReader.ReadInt32();
  }

  public short ReadInt16() {
    CheckNextReadLengthOverflow(2);
    return _binaryReader.ReadInt16();
  }

  public byte ReadByte() {
    CheckNextReadLengthOverflow(1);
    return _binaryReader.ReadByte();
  }

  public bool ReadBoolean() {
    CheckNextReadLengthOverflow(1);
    return _binaryReader.ReadBoolean();
  }

  public string ReadString() {
    var length = _binaryReader.ReadInt32();
    CheckNextReadLengthOverflow(length);

    var bytes = _binaryReader.ReadBytes(length);
    return Encoding.UTF8.GetString(bytes);
  }

  public float ReadSingle() {
    CheckNextReadLengthOverflow(4);
    return _binaryReader.ReadSingle();
  }

  public double ReadDouble() {
    CheckNextReadLengthOverflow(8);
    return _binaryReader.ReadDouble();
  }

  public decimal ReadDecimal() {
    CheckNextReadLengthOverflow(16);
    return _binaryReader.ReadDecimal();
  }

  public uint ReadUInt32() {
    CheckNextReadLengthOverflow(4);
    return _binaryReader.ReadUInt32();
  }

  public ushort ReadUInt16() {
    CheckNextReadLengthOverflow(2);
    return _binaryReader.ReadUInt16();
  }

  public ulong ReadUInt64() {
    CheckNextReadLengthOverflow(8);
    return _binaryReader.ReadUInt64();
  }

  public sbyte ReadSByte() {
    CheckNextReadLengthOverflow(1);
    return _binaryReader.ReadSByte();
  }

  public void Flush() {
    _binaryReader.BaseStream.Flush();
  }


  // public IPacketReadable Read<T>() where T : IPacketReadable, new() {
  //   var instance = new T();
  //   instance.Read(this);
  //   return instance;
  // }
  public byte[] ReadBytes(int messageLength) {
    CheckNextReadLengthOverflow(messageLength);
    return _binaryReader.ReadBytes(messageLength);
  }
}