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

  // public void Write<T>(T value) where T : class, new() {
  //   var properties = typeof(T).GetProperties();
  //   foreach (var property in properties) {
  //     var propertyValue = property.GetValue(value);
  //     if (propertyValue is null) {
  //       //TODO log or throw
  //       continue;
  //     }
  //
  //     var type = property.PropertyType;
  //     switch (type) {
  //       case var _ when type == typeof(bool):
  //         Write((bool)propertyValue);
  //         break;
  //       case var _ when type == typeof(int):
  //         Write((int)propertyValue);
  //         break;
  //       case var _ when type == typeof(ulong):
  //         Write((ulong)propertyValue);
  //         break;
  //       case var _ when type == typeof(long):
  //         Write((long)propertyValue);
  //         break;
  //       case var _ when type == typeof(uint):
  //         Write((uint)propertyValue);
  //         break;
  //       case var _ when type == typeof(short):
  //         Write((short)propertyValue);
  //         break;
  //       case var _ when type == typeof(ushort):
  //         Write((ushort)propertyValue);
  //         break;
  //       case var _ when type == typeof(string):
  //         Write((string)propertyValue);
  //         break;
  //       case var _ when type == typeof(char):
  //         Write((char)propertyValue);
  //         break;
  //       case var _ when type == typeof(sbyte):
  //         Write((sbyte)propertyValue);
  //         break;
  //       case var _ when type == typeof(float):
  //         Write((float)propertyValue);
  //         break;
  //       case var _ when type == typeof(decimal):
  //         Write((decimal)propertyValue);
  //         break;
  //       case var _ when type == typeof(double):
  //         Write((double)propertyValue);
  //         break;
  //       case var _ when type == typeof(byte[]):
  //         Write((byte[])propertyValue);
  //         break;
  //       default:
  //         var isObjectAndClass = type.IsClass && type != typeof(string);
  //         if (isObjectAndClass) {
  //           Write(propertyValue);
  //           break;
  //         }
  //         throw new ArgumentOutOfRangeException(nameof(type), type, null);
  //         break;
  //     }
  //   }
  // }

  // public void Write(IWriteablePacket message) {
  //   message.Write(this);
  // }
  //
  public void Flush() {
    _binaryWriter.BaseStream.Flush();
    _binaryWriter.Flush();
  }
}