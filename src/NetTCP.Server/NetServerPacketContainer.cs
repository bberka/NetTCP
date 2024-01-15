using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using Autofac;
using NetTCP.Abstract;
using NetTCP.Attributes;

namespace NetTCP.Server;

internal record InternalMessageHandlerDelegate(MessageHandlerDelegate Delegate, Type[] ParameterTypes);

internal delegate void MessageHandlerDelegate(NetTcpConnection session, IPacketReadable message, params object[] parameters);

public class NetServerPacketContainer
{
  private IContainer Container { get; set; }

  private delegate IPacketReadable MessageFactoryDelegate();

  internal NetServerPacketContainer() { }


  private static NetServerPacketContainer? _instance;

  private ImmutableDictionary<int, MessageFactoryDelegate> _clientMessageFactories;
  private ImmutableDictionary<Type, int> _serverMessageOpcodes;

  private ImmutableDictionary<int, InternalMessageHandlerDelegate> _clientMessageHandlers;

  private bool _isRegistered = false;

  public bool IsRegistered() => _isRegistered;

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
    var array = types.Distinct().ToArray(); //Remove duplicates
    RegisterMessageHandlers(array);
    RegisterMessages(array);
  }

  private void RegisterMessages(Type[] types) {
    var messageFactories = new Dictionary<int, MessageFactoryDelegate>();
    var messageOpcodes = new Dictionary<Type, int>();

    foreach (var type in types) {
      var attribute = type.GetCustomAttribute<PacketAttribute>();
      if (attribute == null)
        continue;

      if (typeof(IPacketReadable).IsAssignableFrom(type)) {
        var @new = Expression.New(type.GetConstructor(Type.EmptyTypes));
        messageFactories.Add(attribute.MessageId, Expression.Lambda<MessageFactoryDelegate>(@new).Compile());
      }

      if (typeof(IPacketWriteable).IsAssignableFrom(type))
        messageOpcodes.Add(type, attribute.MessageId);
    }

    _clientMessageFactories = messageFactories.ToImmutableDictionary();
    _serverMessageOpcodes = messageOpcodes.ToImmutableDictionary();
  }

  private void RegisterMessageHandlers(Type[] types) {
    var messageHandlers = new Dictionary<int, InternalMessageHandlerDelegate>();

    foreach (var type in types) {
      foreach (var method in type.GetMethods()) {
        if (!type.IsPublic)
          continue;
        if (method.DeclaringType != type)
          continue;
        if (!method.IsStatic) //Only static methods are supported. Instance methods are not supported
          continue;
        var attribute = method.GetCustomAttribute<PacketHandlerAttribute>();
        if (attribute == null)
          continue;

        var parameterInfo = method.GetParameters();
        var parameterTypes = parameterInfo.Select(x => x.ParameterType).ToArray();
        var serviceParamTypes = parameterTypes.Where(x => !x.IsSubclassOf(typeof(NetTcpConnection)) && !x.IsSubclassOf(typeof(IPacketReadable))).ToArray();
        var requiredParamsExists = serviceParamTypes.Length == parameterTypes.Length - 2; //this means either NetTcpConnection or IPacketReadable is missing in params
        if (!requiredParamsExists)
          continue;


        var handlerDelegate = (MessageHandlerDelegate)Delegate.CreateDelegate(typeof(MessageHandlerDelegate), method);
        var internalDelegate = new InternalMessageHandlerDelegate(handlerDelegate, serviceParamTypes);
        messageHandlers.Add(attribute.MessageId, internalDelegate);
      }
    }

    _clientMessageHandlers = messageHandlers.ToImmutableDictionary();
  }

  internal IPacketReadable GetMessage(int messageId) {
    return _clientMessageFactories.TryGetValue(messageId, out MessageFactoryDelegate factory)
             ? factory.Invoke()
             : null;
  }

  internal bool GetOpcode(IPacketWriteable message, out int messageId) {
    return _serverMessageOpcodes.TryGetValue(message.GetType(), out messageId);
  }

  internal bool Validate(IContainer container) {
    Container = container;
    var allServicesRequired = _clientMessageHandlers.Values.All(x => x.ParameterTypes.All(y => container.TryResolve(y, out _)));
    if (!allServicesRequired)
      return false;
    return true;
  }

  internal void InvokeHandler(int messageId, NetTcpConnection connection, IPacketReadable packet) {
    if (!_clientMessageHandlers.TryGetValue(messageId, out var handlerDelegate)) {
      //TODO Throw or log or call events
      return;
    }

    var dlg = handlerDelegate.Delegate;
    var types = handlerDelegate.ParameterTypes;
    var additionalParameters = new object[types.Length];
    using var scope = Container.BeginLifetimeScope();
    for (var i = 0; i < types.Length; i++) {
      additionalParameters[i] = scope.Resolve(types[i]);
    }

    dlg(connection, packet, additionalParameters);
  }
}