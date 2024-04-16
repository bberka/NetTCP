using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Xml.Serialization;
using NetTCP.Abstract;
using Newtonsoft.Json;

namespace NetTCP.Network;

public sealed class TcpPacketReader
{
  private readonly BinaryReader _reader;

  public TcpPacketReader(BinaryReader reader) {
    _reader = reader;
  }

  public TcpPacketReader(byte[] buffer) {
    _reader = new BinaryReader(new MemoryStream(buffer));
  }
  public TimeSpan ReadTimeSpan() => TimeSpan.FromTicks(_reader.ReadInt64());

  public DateTimeOffset ReadDateTimeOffset() => DateTimeOffset.FromUnixTimeMilliseconds(_reader.ReadInt64());

  public DateTime ReadDateTime() => DateTime.FromBinary(_reader.ReadInt64());

  public Guid ReadGuid() => new Guid(_reader.ReadBytes(16));

  public uint ReadUInt() => _reader.ReadUInt32();

  public int ReadInt() => _reader.ReadInt32();

  public ushort ReadUShort() => _reader.ReadUInt16();

  public short ReadShort() => _reader.ReadInt16();

  public ulong ReadULong() => _reader.ReadUInt64();

  public long ReadLong() => _reader.ReadInt64();

  public char ReadChar() => _reader.ReadChar();

  public bool ReadBool() => _reader.ReadBoolean();

  public float ReadFloat() => _reader.ReadSingle();

  public decimal ReadDecimal() => _reader.ReadDecimal();

  public double ReadDouble() => _reader.ReadDouble();

  public byte ReadByte() => _reader.ReadByte();

  public Vector2 ReadVector2() => new Vector2(_reader.ReadSingle(), _reader.ReadSingle());

  public Vector3 ReadVector3() => new Vector3(_reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle());

  public Vector4 ReadVector4() => new Vector4(_reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle());
  public IEnumerable<byte> ReadEnumerableByte() => _reader.ReadBytes(_reader.ReadInt32());

  public string ReadString() {
    var length = _reader.ReadInt32();
    return length == 0
             ? string.Empty
             : _reader.ReadString();
  }

  public IPacket ReadPacket<T>() where T : IPacket, new() {
    var packet = new T();
    packet.Read(this);
    return packet;
  }

  public IEnumerable<string> ReadEnumerableString() {
    var length = _reader.ReadInt32();
    var buffer = new string[length];
    for (var i = 0; i < length; i++) {
      buffer[i] = ReadString();
    }

    return buffer;
  }

  public IEnumerable<IPacket> ReadEnumerablePacket<T>() where T : IPacket, new() {
    var length = _reader.ReadInt32();
    var buffer = new IPacket[length];
    for (var i = 0; i < length; i++) {
      buffer[i] = ReadPacket<T>();
    }

    return buffer;
  }

  public IEnumerable<int> ReadEnumerableInt() {
    var length = _reader.ReadInt32();
    var buffer = new int[length];
    for (var i = 0; i < length; i++) {
      buffer[i] = ReadInt();
    }

    return buffer;
  }

  public IEnumerable<uint> ReadEnumerableUInt() {
    var length = _reader.ReadInt32();
    var buffer = new uint[length];
    for (var i = 0; i < length; i++) {
      buffer[i] = ReadUInt();
    }

    return buffer;
  }

  public IEnumerable<long> ReadEnumerableLong() {
    var length = _reader.ReadInt32();
    var buffer = new long[length];
    for (var i = 0; i < length; i++) {
      buffer[i] = ReadLong();
    }

    return buffer;
  }

  public IEnumerable<ulong> ReadEnumerableULong() {
    var length = _reader.ReadInt32();
    var buffer = new ulong[length];
    for (var i = 0; i < length; i++) {
      buffer[i] = ReadULong();
    }

    return buffer;
  }

  public IEnumerable<short> ReadEnumerableShort() {
    var length = _reader.ReadInt32();
    var buffer = new short[length];
    for (var i = 0; i < length; i++) {
      buffer[i] = ReadShort();
    }

    return buffer;
  }

  public IEnumerable<ushort> ReadEnumerableUShort() {
    var length = _reader.ReadInt32();
    var buffer = new ushort[length];
    for (var i = 0; i < length; i++) {
      buffer[i] = ReadUShort();
    }

    return buffer;
  }

  public IEnumerable<float> ReadEnumerableFloat() {
    var length = _reader.ReadInt32();
    var buffer = new float[length];
    for (var i = 0; i < length; i++) {
      buffer[i] = ReadFloat();
    }

    return buffer;
  }

  public IEnumerable<double> ReadEnumerableDouble() {
    var length = _reader.ReadInt32();
    var buffer = new double[length];
    for (var i = 0; i < length; i++) {
      buffer[i] = ReadDouble();
    }

    return buffer;
  }

  public IEnumerable<decimal> ReadEnumerableDecimal() {
    var length = _reader.ReadInt32();
    var buffer = new decimal[length];
    for (var i = 0; i < length; i++) {
      buffer[i] = ReadDecimal();
    }

    return buffer;
  }

  public IEnumerable<bool> ReadEnumerableBool() {
    var length = _reader.ReadInt32();
    var buffer = new bool[length];
    for (var i = 0; i < length; i++) {
      buffer[i] = ReadBool();
    }

    return buffer;
  }

  public IEnumerable<char> ReadEnumerableChar() {
    var length = _reader.ReadInt32();
    var buffer = new char[length];
    for (var i = 0; i < length; i++) {
      buffer[i] = ReadChar();
    }

    return buffer;
  }

  public IEnumerable<Guid> ReadEnumerableGuid() {
    var length = _reader.ReadInt32();
    var buffer = new Guid[length];
    for (var i = 0; i < length; i++) {
      buffer[i] = ReadGuid();
    }

    return buffer;
  }

  public IEnumerable<TimeSpan> ReadEnumerableTimeSpan() {
    var length = _reader.ReadInt32();
    var buffer = new TimeSpan[length];
    for (var i = 0; i < length; i++) {
      buffer[i] = ReadTimeSpan();
    }

    return buffer;
  }

  public IEnumerable<DateTime> ReadEnumerableDateTime() {
    var length = _reader.ReadInt32();
    var buffer = new DateTime[length];
    for (var i = 0; i < length; i++) {
      buffer[i] = ReadDateTime();
    }

    return buffer;
  }


  public IEnumerable<DateTimeOffset> ReadEnumerableDateTimeOffset() {
    var length = _reader.ReadInt32();
    var buffer = new DateTimeOffset[length];
    for (var i = 0; i < length; i++) {
      buffer[i] = ReadDateTimeOffset();
    }

    return buffer;
  }

  public IEnumerable<Vector2> ReadEnumerableVector2() {
    var length = _reader.ReadInt32();
    var buffer = new Vector2[length];
    for (var i = 0; i < length; i++) {
      buffer[i] = ReadVector2();
    }

    return buffer;
  }

  public IEnumerable<Enum> ReadEnumerableEnum() {
    var length = _reader.ReadInt32();
    var buffer = new Enum[length];
    for (var i = 0; i < length; i++) {
      buffer[i] = ReadEnum(typeof(Enum));
    }

    return buffer;
  }

  public IEnumerable<Vector3> ReadEnumerableVector3() {
    var length = _reader.ReadInt32();
    var buffer = new Vector3[length];
    for (var i = 0; i < length; i++) {
      buffer[i] = ReadVector3();
    }

    return buffer;
  }

  public IEnumerable<Vector4> ReadEnumerableVector4() {
    var length = _reader.ReadInt32();
    var buffer = new Vector4[length];
    for (var i = 0; i < length; i++) {
      buffer[i] = ReadVector4();
    }

    return buffer;
  }

  public T ReadAsJson<T>(Encoding encoding = null, JsonSerializerSettings settings = null) {
    var buffer = ReadEnumerableByte().ToArray();
    var str = encoding?.GetString(buffer) ?? Encoding.UTF8.GetString(buffer);
    return JsonConvert.DeserializeObject<T>(str, settings);
  }

  public T ReadAsXml<T>(Encoding encoding = null) {
    var buffer = ReadEnumerableByte().ToArray();
    using var stream = new MemoryStream(buffer);
    var serializer = new XmlSerializer(typeof(T));
    return (T)serializer.Deserialize(stream);
  }

  public Enum ReadEnum(Type type) {
    return (Enum)Enum.ToObject(type, ReadInt());
  }

  public T ReadEnum<T>() where T : Enum {
    return (T)Enum.ToObject(typeof(T), ReadInt());
  }

  public T ReadEnum<T>(T defaultValue) where T : Enum {
    return Enum.IsDefined(typeof(T), ReadInt())
             ? (T)Enum.ToObject(typeof(T), ReadInt())
             : defaultValue;
  }
}