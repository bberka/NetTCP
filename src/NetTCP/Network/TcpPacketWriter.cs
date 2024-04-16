using System.Numerics;
using System.Text;
using System.Xml.Serialization;
using NetTCP.Abstract;
using Newtonsoft.Json;

namespace NetTCP.Network;

public sealed class TcpPacketWriter
{
  private readonly BinaryWriter _binaryWriter;

  public TcpPacketWriter() {
    _binaryWriter = new BinaryWriter(new MemoryStream());
  }
  public void Write(TimeSpan value) => _binaryWriter.Write(value.Ticks);
  public void Write(DateTimeOffset value) => _binaryWriter.Write(value.Ticks);
  public void Write(DateTime value) => _binaryWriter.Write(value.Ticks);
  public void Write(Guid value) => _binaryWriter.Write(value.ToByteArray());
  public void Write(uint value) => _binaryWriter.Write(value);
  public void Write(int value) => _binaryWriter.Write(value);

  public void Write(ushort value) => _binaryWriter.Write(value);
  public void Write(short value) => _binaryWriter.Write(value);

  public void Write(ulong value) => _binaryWriter.Write(value);
  public void Write(long value) => _binaryWriter.Write(value);

  public void Write(char value) => _binaryWriter.Write(value);

  public void Write(bool value) => _binaryWriter.Write(value);

  public void Write(float value) => _binaryWriter.Write(value);

  public void Write(decimal value) => _binaryWriter.Write(value);

  public void Write(double value) => _binaryWriter.Write(value);

  public void Write(byte value) => _binaryWriter.Write(value);

  public void Write(Vector2 value) {
    _binaryWriter.Write(value.X);
    _binaryWriter.Write(value.Y);
  }

  public void Write(Vector3 value) {
    _binaryWriter.Write(value.X);
    _binaryWriter.Write(value.Y);
    _binaryWriter.Write(value.Z);
  }

  public void Write(Vector4 value) {
    _binaryWriter.Write(value.X);
    _binaryWriter.Write(value.Y);
    _binaryWriter.Write(value.Z);
    _binaryWriter.Write(value.W);
  }

  public void Write(string value) {
    if (value == null || value.Length == 0) {
      return;
    }

    var length = value.Length;
    _binaryWriter.Write(length);
    _binaryWriter.Write(value);
  }


  public void Write(IPacket packet) {
    packet.Write(this);
  }


  public void Write(IEnumerable<string> buffer) {
    if (buffer == null) {
      return;
    }

    var array = buffer.ToArray();
    var length = array.Length;
    _binaryWriter.Write(length);
    foreach (var value in array) {
      Write(value);
    }
  }

  public void Write(IEnumerable<byte> buffer) {
    if (buffer == null) {
      return;
    }

    var array = buffer.ToArray();
    var length = array.Length;
    _binaryWriter.Write(length);
    foreach (var value in array) {
      Write(value);
    }
  }

  public void Write(IEnumerable<IPacket> buffer) {
    if (buffer == null) {
      return;
    }

    var array = buffer.ToArray();
    var length = array.Length;
    _binaryWriter.Write(length);
    foreach (var value in array) {
      Write(value);
    }
  }

  public void Write(IEnumerable<int> buffer) {
    if (buffer == null) {
      return;
    }

    var array = buffer.ToArray();
    var length = array.Length;
    _binaryWriter.Write(length);
    foreach (var value in array) {
      Write(value);
    }
  }

  public void Write(IEnumerable<uint> buffer) {
    if (buffer == null) {
      return;
    }

    var array = buffer.ToArray();
    var length = array.Length;
    _binaryWriter.Write(length);
    foreach (var value in array) {
      Write(value);
    }
  }

  public void Write(IEnumerable<long> buffer) {
    if (buffer == null) {
      return;
    }

    var array = buffer.ToArray();
    var length = array.Length;
    _binaryWriter.Write(length);
    foreach (var value in array) {
      Write(value);
    }
  }

  public void Write(IEnumerable<ulong> buffer) {
    if (buffer == null) {
      return;
    }

    var array = buffer.ToArray();
    var length = array.Length;
    _binaryWriter.Write(length);
    foreach (var value in array) {
      Write(value);
    }
  }

  public void Write(IEnumerable<short> buffer) {
    if (buffer == null) {
      return;
    }

    var array = buffer.ToArray();
    var length = array.Length;
    _binaryWriter.Write(length);
    foreach (var value in array) {
      Write(value);
    }
  }

  public void Write(IEnumerable<ushort> buffer) {
    if (buffer == null) {
      return;
    }

    var array = buffer.ToArray();
    var length = array.Length;
    _binaryWriter.Write(length);
    foreach (var value in array) {
      Write(value);
    }
  }

  public void Write(IEnumerable<float> buffer) {
    if (buffer == null) {
      return;
    }

    var array = buffer.ToArray();
    var length = array.Length;
    _binaryWriter.Write(length);
    foreach (var value in array) {
      Write(value);
    }
  }

  public void Write(IEnumerable<double> buffer) {
    if (buffer == null) {
      return;
    }

    var array = buffer.ToArray();
    var length = array.Length;
    _binaryWriter.Write(length);
    foreach (var value in array) {
      Write(value);
    }
  }

  public void Write(IEnumerable<decimal> buffer) {
    if (buffer == null) {
      return;
    }

    var array = buffer.ToArray();
    var length = array.Length;
    _binaryWriter.Write(length);
    foreach (var value in array) {
      Write(value);
    }
  }

  public void Write(IEnumerable<bool> buffer) {
    if (buffer == null) {
      return;
    }

    var array = buffer.ToArray();
    var length = array.Length;
    _binaryWriter.Write(length);
    foreach (var value in array) {
      Write(value);
    }
  }

  public void Write(IEnumerable<char> buffer) {
    if (buffer == null) {
      return;
    }

    var array = buffer.ToArray();
    var length = array.Length;
    _binaryWriter.Write(length);
    foreach (var value in array) {
      Write(value);
    }
  }


  public void Write(IEnumerable<Guid> buffer) {
    if (buffer == null) {
      return;
    }

    var array = buffer.ToArray();
    var length = array.Length;
    _binaryWriter.Write(length);
    foreach (var value in array) {
      Write(value);
    }
  }

  public void Write(IEnumerable<DateTime> buffer) {
    if (buffer == null) {
      return;
    }

    var array = buffer.ToArray();
    var length = array.Length;
    _binaryWriter.Write(length);
    foreach (var value in array) {
      Write(value);
    }
  }

  public void Write(IEnumerable<DateTimeOffset> buffer) {
    if (buffer == null) {
      return;
    }

    var array = buffer.ToArray();
    var length = array.Length;
    _binaryWriter.Write(length);
    foreach (var value in array) {
      Write(value);
    }
  }

  public void Write(IEnumerable<TimeSpan> buffer) {
    if (buffer == null) {
      return;
    }

    var array = buffer.ToArray();
    var length = array.Length;
    _binaryWriter.Write(length);
    foreach (var value in array) {
      Write(value);
    }
  }


  public void Write(Enum value) {
    Write(Convert.ToInt32(value));
  }

  public void Write(IEnumerable<Enum> value) {
    if (value == null) {
      return;
    }

    var array = value.ToArray();
    var length = array.Length;
    _binaryWriter.Write(length);
    foreach (var item in array) {
      Write(item);
    }
  }

  public void Write(IEnumerable<Vector2> value) {
    if (value == null) {
      return;
    }

    var array = value.ToArray();
    var length = array.Length;
    _binaryWriter.Write(length);
    foreach (var item in array) {
      Write(item);
    }
  }

  public void Write(IEnumerable<Vector3> value) {
    if (value == null) {
      return;
    }

    var array = value.ToArray();
    var length = array.Length;
    _binaryWriter.Write(length);
    foreach (var item in array) {
      Write(item);
    }
  }

  public void Write(IEnumerable<Vector4> value) {
    if (value == null) {
      return;
    }

    var array = value.ToArray();
    var length = array.Length;
    _binaryWriter.Write(length);
    foreach (var item in array) {
      Write(item);
    }
  }

  public void WriteAsJson<T>(T value, Encoding encoding = null, JsonSerializerSettings settings = null) {
    var str = JsonConvert.SerializeObject(value, settings);
    var buffer = encoding?.GetBytes(str) ?? Encoding.UTF8.GetBytes(str);
    Write(buffer);
  }

  public void WriteAsXml<T>(T value, Encoding encoding = null) {
    var serializer = new XmlSerializer(value.GetType());
    using var stream = new MemoryStream();
    serializer.Serialize(stream, value);
    Write(stream.ToArray());
  }
  
  public byte[] ToArray() {
    return ((MemoryStream)_binaryWriter.BaseStream).ToArray();
  }
}

