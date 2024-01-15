using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using Autofac;
using NetTCP.Abstract;
using NetTCP.Attributes;

namespace NetTCP.Server;

internal delegate void MessageHandlerDelegate(NetTcpConnection connection, IReadablePacket readablePacket, ILifetimeScope scope);

public class NetServerPacketContainer
{
  private IContainer Container { get; set; }

  private delegate IReadablePacket MessageFactoryDelegate();

  internal NetServerPacketContainer() { }


  private static NetServerPacketContainer? _instance;

  private ImmutableDictionary<int, MessageFactoryDelegate> _clientMessageFactories;
  private ImmutableDictionary<Type, int> _serverMessageOpcodes;

  private ImmutableDictionary<int, MessageHandlerDelegate> _clientMessageHandlers;

  private bool _isRegistered = false;

  private Type _packetAttributeType = typeof(PacketAttribute);
  private Type _packetHandlerAttributeType = typeof(PacketHandlerAttribute);
  private Type _netTcpConnectionType = typeof(NetTcpConnection);
  private Type _iReadablePacketType = typeof(IReadablePacket);
  private Type _iWriteablePacketType = typeof(IWriteablePacket);

  internal bool IsRegistered() => _isRegistered;

  /// <summary>
  /// Register all message handlers and messages from an assembly
  /// It will also get all types from the entry assembly
  /// If you provide an assembly it will also get all types from that assembly and concat them
  /// </summary>
  /// <param name="assembly"></param>
  internal void Register(Assembly? assembly = null) {
    if (assembly is null) {
      Register(new[] {
        Assembly.GetEntryAssembly()
      });
      return;
    }

    Register(new[] {
      Assembly.GetEntryAssembly(),
      assembly
    });
  }

  /// <summary>
  /// Register all message handlers and messages from an assembly
  /// It will also get all types from the entry assembly
  /// If you provide an assembly it will also get all types from that assembly and concat them
  /// </summary>
  /// <param name="assembly"></param>
  internal void Register(Assembly[] assembly) {
    if (_isRegistered) throw new InvalidOperationException("Packet Container is already registered");
    var types = assembly.SelectMany(x => x.GetTypes()).ToList();
    types.AddRange(Assembly.GetEntryAssembly().GetTypes());
    var array = types.DistinctBy(x => x.FullName).ToArray(); //Remove duplicates
    RegisterMessageHandlers(array);
    RegisterMessages(array);
    foreach (var factory in _clientMessageFactories) {
      var hasHandler = _clientMessageHandlers.ContainsKey(factory.Key);
      if (!hasHandler) {
        throw new Exception($"Message with opcode {factory.Key} has no handler");
      }
    }
  }

  private void RegisterMessages(Type[] types) {
    var messageFactories = new Dictionary<int, MessageFactoryDelegate>();
    var messageOpcodes = new Dictionary<Type, int>();

    foreach (var type in types) {
      var attribute = type.GetCustomAttribute<PacketAttribute>();
      if (attribute == null)
        continue;

      var isWriteable = type.GetInterface(nameof(IWriteablePacket)) != null;
      var isReadable = type.GetInterface(nameof(IReadablePacket)) != null;
      if (isWriteable) {
        messageOpcodes.Add(type, attribute.MessageId);
        continue;
      }

      if (isReadable) {
        var @new = Expression.New(type.GetConstructor(Type.EmptyTypes));
        messageFactories.Add(attribute.MessageId, Expression.Lambda<MessageFactoryDelegate>(@new).Compile());
        continue;
      }

      //TODO maybe log ? 
    }

    _clientMessageFactories = messageFactories.ToImmutableDictionary();
    _serverMessageOpcodes = messageOpcodes.ToImmutableDictionary();
  }

  private void RegisterMessageHandlers(Type[] types) {
    var messageHandlers = new Dictionary<int, MessageHandlerDelegate>();

    foreach (var type in types.Where(x => x.IsPublic)) {
      foreach (var method in type.GetMethods().Where(x => x.IsPublic && x.IsStatic && x.DeclaringType != type)) {
        var attribute = method.GetCustomAttribute<PacketHandlerAttribute>();
        if (attribute == null)
          continue;
        var handlerDelegate = BuildMessageHandlerDelegate(method);
        messageHandlers.Add(attribute.MessageId, handlerDelegate);
      }
    }

    _clientMessageHandlers = messageHandlers.ToImmutableDictionary();
  }

  private MessageHandlerDelegate BuildMessageHandlerDelegate(MethodInfo method) {
    var sessionParameter = Expression.Parameter(typeof(NetTcpConnection));
    var messageParameter = Expression.Parameter(typeof(IReadablePacket));
    var scopeParameter = Expression.Parameter(typeof(ILifetimeScope));
    var parameterInfo = method.GetParameters();
    var sessionParamInfo = parameterInfo.Single(x => x.ParameterType == typeof(NetTcpConnection));
    var messageParamInfo = parameterInfo.Single(x => x.ParameterType.GetInterface(nameof(IReadablePacket)) != null);
    var scopeParamInfo = parameterInfo.Single(x => x.ParameterType == typeof(ILifetimeScope));
    var call = Expression.Call(method,
                               Expression.Convert(sessionParameter, sessionParamInfo.ParameterType),
                               Expression.Convert(messageParameter, messageParamInfo.ParameterType),
                               Expression.Convert(scopeParameter, scopeParamInfo.ParameterType));
    var lambda = Expression.Lambda<MessageHandlerDelegate>(call, sessionParameter, messageParameter, scopeParameter);
    return lambda.Compile();
  }


  internal IReadablePacket GetMessage(int messageId) {
    return _clientMessageFactories.TryGetValue(messageId, out MessageFactoryDelegate factory)
             ? factory.Invoke()
             : null;
  }

  internal bool GetOpcode(IWriteablePacket message, out int messageId) {
    return _serverMessageOpcodes.TryGetValue(message.GetType(), out messageId);
  }

  internal void InvokeHandler(int messageId, NetTcpConnection connection, IReadablePacket readablePacket) {
    if (!_clientMessageHandlers.TryGetValue(messageId, out var handlerDelegate)) {
      //TODO Throw or log or call events
      return;
    }

    using (var scope = Container.BeginLifetimeScope()) {
      handlerDelegate.Invoke(connection, readablePacket, scope);
    }
  }

  internal void InitDependencyContainer(IContainer container) {
    Container = container;
  }
}