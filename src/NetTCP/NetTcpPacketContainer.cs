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
  protected ImmutableDictionary<Type, int> OpCodes;

  private IContainer Container { get; set; }


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
    types = types.DistinctBy(x => x.FullName).ToArray(); //Remove duplicates
    var messageHandlers = new Dictionary<int, MessageHandlerDelegate>();

    foreach (var type in types.Where(x => x.IsPublic))
    foreach (var method in type.GetMethods().Where(x => x.IsPublic && x.IsStatic && x.DeclaringType == type)) {
      var attribute = method.GetCustomAttribute<PacketHandlerAttribute>();
      if (attribute == null)
        continue;
      var handlerDelegate = BuildMessageHandlerDelegate(method);
      var added = messageHandlers.TryAdd(attribute.MessageId, handlerDelegate);
      if (added == false)
        throw new Exception($"Message with opcode {attribute.MessageId} has multiple handlers");
    }

    MessageHandlers = messageHandlers.ToImmutableDictionary();
    Console.WriteLine($"Registered {messageHandlers.Count} message handlers to handle received packets");


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

    Console.WriteLine($"Registered {messageFactories.Count} message factories to read received packets");
    Console.WriteLine($"Registered {messageOpcodes.Count} opcodes for packets that will be sent");
    MessageFactories = messageFactories.ToImmutableDictionary();
    OpCodes = messageOpcodes.ToImmutableDictionary();

    foreach (var factory in MessageFactories) {
      var hasHandler = MessageHandlers.ContainsKey(factory.Key);
      if (!hasHandler) {
        //TODO LOG or TRIGGER EVENT
        // throw new Exception($"Message with opcode {factory.Key} has no handler");
      }
    }
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

  public virtual void InvokeHandler(int messageId, T connection, IPacket packet) {
    if (!MessageHandlers.TryGetValue(messageId, out var handlerDelegate)) {
      throw new Exception($"Message with opcode {messageId} has no handler");
      return;
    }

    using (var scope = Container.BeginLifetimeScope()) {
      handlerDelegate.Invoke(connection, packet, scope);
    }
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