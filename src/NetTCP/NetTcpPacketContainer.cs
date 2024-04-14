using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;
using Autofac;
using NetTCP.Abstract;
using NetTCP.Attributes;

namespace NetTCP;

public abstract class NetTcpPacketContainer<T> where T : class
{
  private static NetTcpPacketContainer<T>? _instance;

  protected ImmutableDictionary<int, MessageFactoryDelegate> MessageFactories;

  protected ImmutableDictionary<int, MessageHandlerDelegate> MessageHandlers;
  private IContainer Container { get; set; }

  public ImmutableDictionary<Type, int> OpCodes { get; protected set; }

  private List<Assembly> Assemblies { get; } = new();

  /// <summary>
  ///   Register all message handlers and messages from an assembly
  ///   It will also get all types from the entry assembly
  ///   If you provide an assembly it will also get all types from that assembly and concat them
  /// </summary>
  /// <param name="assembly"></param>
  public void Register(Assembly assembly) {
    Assemblies.Add(assembly);
  }


  /// <summary>
  ///   Register all message handlers and messages from an assembly
  ///   It will also get all types from the entry assembly
  ///   If you provide an assembly it will also get all types from that assembly and concat them
  /// </summary>
  /// <param name="assembly"></param>
  private void InitializeMessageTypes(Type[] types) {
    // types = types.Select(x => new {
    //                Type = x,
    //                Methods = x.GetMethods(),
    //                FullName = x.FullName,
    //              })
    //              .DistinctBy(x => x.FullName)
    //              .Select(x => x.Type)
    //              .ToArray(); 

    var messageHandlers = new Dictionary<int, MessageHandlerDelegate>();

    foreach (var type in types.Where(x => x.IsPublic))
    foreach (var method in type.GetMethods().Where(x => x.IsPublic && x.IsStatic && x.DeclaringType == type)) {
      var parameterInfo = method.GetParameters();
      var is3Params = parameterInfo.Length == 3;
      if (!is3Params)
        continue;
      var sessionParam = parameterInfo.FirstOrDefault(x => x.ParameterType == typeof(T));
      var packetParam = parameterInfo.FirstOrDefault(x => x.ParameterType.GetInterface(nameof(IPacket)) != null);
      var scopeParam = parameterInfo.FirstOrDefault(x => x.ParameterType == typeof(ILifetimeScope));
      if (sessionParam == null || packetParam == null || scopeParam == null)
        continue;
      var attribute = packetParam.ParameterType.GetCustomAttribute<PacketAttribute>();
      if (attribute == null)
        continue;
      if (messageHandlers.ContainsKey(attribute.MessageId))
        continue; //Maybe log this?
      var handlerDelegate = BuildMessageHandlerDelegate(method);
      messageHandlers.Add(attribute.MessageId, handlerDelegate);
    }

    MessageHandlers = messageHandlers.ToImmutableDictionary();


    var messageFactories = new Dictionary<int, MessageFactoryDelegate>();
    var messageOpcodes = new Dictionary<Type, int>();

    foreach (var type in types) {
      var attribute = type.GetCustomAttribute<PacketAttribute>();
      if (attribute == null)
        continue;
      var isPacket = type.GetInterface(nameof(IPacket)) != null;
      if (isPacket) {
        var handlerExists = MessageHandlers.ContainsKey(attribute.MessageId);
        if (handlerExists) {
          var @new = Expression.New(type.GetConstructor(Type.EmptyTypes));
          var lambda = Expression.Lambda<MessageFactoryDelegate>(@new);
          var factory = lambda.Compile();
          messageFactories.Add(attribute.MessageId, factory);
        }
        else {
          messageOpcodes.Add(type, attribute.MessageId);
        }
      }
    }

    MessageFactories = messageFactories.ToImmutableDictionary();
    OpCodes = messageOpcodes.ToImmutableDictionary();
  }


  private MessageHandlerDelegate BuildMessageHandlerDelegate(MethodInfo method) {
    var sessionParameter = Expression.Parameter(typeof(T));
    var messageParameter = Expression.Parameter(typeof(IPacket));
    var scopeParameter = Expression.Parameter(typeof(ILifetimeScope));
    var parameterInfo = method.GetParameters();
    var sessionParamInfo = parameterInfo.Single(x => x.ParameterType == typeof(T));
    var messageParamInfo = parameterInfo.Single(x => x.ParameterType.GetInterface(nameof(IPacket)) != null);
    var scopeParamInfo = parameterInfo.Single(x => x.ParameterType == typeof(ILifetimeScope));
    var call = Expression.Call(method,
                               Expression.Convert(sessionParameter, sessionParamInfo.ParameterType),
                               Expression.Convert(messageParameter, messageParamInfo.ParameterType),
                               Expression.Convert(scopeParameter, scopeParamInfo.ParameterType));
    var lambda = Expression.Lambda<MessageHandlerDelegate>(call, sessionParameter, messageParameter, scopeParameter);
    return lambda.Compile();
  }


  public virtual IPacket GetMessage(int messageId) {
    return MessageFactories.TryGetValue(messageId, out var factory)
             ? factory.Invoke()
             : null;
  }


  public virtual bool GetOpcode(IPacket message, out int messageId) {
    return OpCodes.TryGetValue(message.GetType(), out messageId);
  }

  public virtual bool InvokeHandler(int messageId, T connection, IPacket packet) {
    if (!MessageHandlers.TryGetValue(messageId, out var handlerDelegate)) {
      // throw new Exception($"Message with opcode {messageId} has no handler");
      return false;
    }

    using (var scope = Container.BeginLifetimeScope()) {
      handlerDelegate.Invoke(connection, packet, scope);
    }

    return true;
  }

  public void InitializeBuild(IContainer container) {
    Container = container;
    Assemblies.Add(Assembly.GetEntryAssembly());
    var types = Assemblies.SelectMany(GetTypes).ToArray();
    InitializeMessageTypes(types);
  }

  private Type[] GetTypes(Assembly assembly) {
    if (assembly is null) return Array.Empty<Type>();
    return assembly.GetTypes();
  }

  protected delegate void MessageHandlerDelegate(T connection, IPacket packet, ILifetimeScope scope);

  protected delegate IPacket MessageFactoryDelegate();
}