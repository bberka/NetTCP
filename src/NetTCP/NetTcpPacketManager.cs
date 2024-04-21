using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Autofac;
using NetTCP.Abstract;
using NetTCP.Attributes;

namespace NetTCP;

public sealed class NetTcpPacketManager<T> where T : INetTcpSession
{
  public PacketManagerType ManagerType { get; }

  public delegate void MessageHandlerDelegate(T connection, IPacket packet);

  protected internal delegate IPacket MessageFactoryDelegate();

  protected ImmutableDictionary<int, MessageFactoryDelegate> MessageFactories;

  protected ImmutableDictionary<int, MessageHandlerDelegate> MessageHandlers;
  private IContainer Container { get; set; }

  public ImmutableDictionary<Type, int> OpCodes { get; protected set; }

  private HashSet<Assembly> Assemblies { get; } = new();

  private bool _isInitialized = false;

  public NetTcpPacketManager(PacketManagerType managerType) {
    ManagerType = managerType;
  }

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
  public void Initialize() {
    if (_isInitialized) {
      return;
    }

    _isInitialized = true;
    Assemblies.Add(Assembly.GetEntryAssembly());
    var types = Assemblies.SelectMany(x => x.GetTypes());

    MessageFactories = ImmutableDictionary<int, MessageFactoryDelegate>.Empty;
    MessageHandlers = ImmutableDictionary<int, MessageHandlerDelegate>.Empty;
    OpCodes = ImmutableDictionary<Type, int>.Empty;

    lock (MessageHandlers) {
      var messageHandlers = new Dictionary<int, MessageHandlerDelegate>();
      foreach (var type in types.Where(x => x.IsPublic))
      foreach (var method in type.GetMethods().Where(x => x.IsPublic && x.DeclaringType == type && x.IsStatic)) {
        var handlerDelegate = CreateHandler(method, out var messageId);
        if (handlerDelegate is null) {
          continue;
        }

        if (messageHandlers.ContainsKey(messageId)) {
          throw new InvalidOperationException($"Duplicate message handler found. Please check your message handlers. MessageId: {messageId} TypeName: {type.FullName}");
          continue;
        }
        messageHandlers.Add(messageId, handlerDelegate);
      }

      MessageHandlers = messageHandlers.ToImmutableDictionary();
      Debug.WriteLine($"Message handlers registered successfully. Count: {MessageHandlers.Count}", nameof(NetTcpPacketManager<T>));
    }


    lock (MessageFactories) {
      lock (OpCodes) {
        var messageFactories = new Dictionary<int, MessageFactoryDelegate>();
        var messageOpcodes = new Dictionary<Type, int>();

        foreach (var type in types) {
          var attribute = type.GetCustomAttribute<PacketAttribute>();
          if (attribute == null)
            continue;
          var isPacket = type.GetInterface(nameof(IPacket)) != null;
          if (!isPacket) {
            continue;
          }

          switch (ManagerType) {
            case PacketManagerType.Client:
              var canSendByClient = attribute.Type == PacketType.Client || attribute.Type == PacketType.ClientAndServer;
              if (canSendByClient) {
                messageOpcodes.Add(type, attribute.MessageId);
              }

              var canReceiveByClient = attribute.Type == PacketType.Server || attribute.Type == PacketType.ClientAndServer;
              if (canReceiveByClient) {
                var handlerExists = MessageHandlers.ContainsKey(attribute.MessageId);
                if (handlerExists) {
                  var @new = Expression.New(type.GetConstructor(Type.EmptyTypes));
                  var lambda = Expression.Lambda<MessageFactoryDelegate>(@new);
                  var factory = lambda.Compile();
                  messageFactories.Add(attribute.MessageId, factory);
                }
              }
              break;
            case PacketManagerType.Server:
              var canSendByServer = attribute.Type == PacketType.Server || attribute.Type == PacketType.ClientAndServer;
              if (canSendByServer) {
                messageOpcodes.Add(type, attribute.MessageId);
              }
              
              var canReceiveByServer = attribute.Type == PacketType.Client || attribute.Type == PacketType.ClientAndServer;
              if (canReceiveByServer) {
                var handlerExists = MessageHandlers.ContainsKey(attribute.MessageId);
                if (handlerExists) {
                  var @new = Expression.New(type.GetConstructor(Type.EmptyTypes));
                  var lambda = Expression.Lambda<MessageFactoryDelegate>(@new);
                  var factory = lambda.Compile();
                  messageFactories.Add(attribute.MessageId, factory);
                }
              }
               
              break;
            default:
              continue;
          }

        }

        MessageFactories = messageFactories.ToImmutableDictionary();
        OpCodes = messageOpcodes.ToImmutableDictionary();
        Debug.WriteLine($"Message factories registered successfully. Count: {MessageFactories.Count}", nameof(NetTcpPacketManager<T>));
        Debug.WriteLine($"Message opcodes registered successfully. Count: {OpCodes.Count}", nameof(NetTcpPacketManager<T>));
      }
    }
  }

  private MessageHandlerDelegate? CreateHandler(MethodInfo method, out int messageId) {
    messageId = -1;
    var parameterInfo = method.GetParameters();
    var sessionParam = parameterInfo.FirstOrDefault(x => x.ParameterType == typeof(T));
    var packetParam = parameterInfo.FirstOrDefault(x => x.ParameterType.GetInterface(nameof(IPacket)) != null);
    if (sessionParam == null || packetParam == null)
      return null;
    var attribute = packetParam.ParameterType.GetCustomAttribute<PacketAttribute>();
    if (attribute == null)
      return null;
    messageId = attribute.MessageId;

    var sessionParameter = Expression.Parameter(typeof(T));
    var messageParameter = Expression.Parameter(typeof(IPacket));
    var sessionParamInfo = parameterInfo.Single(x => x.ParameterType == typeof(T));
    var messageParamInfo = parameterInfo.Single(x => x.ParameterType.GetInterface(nameof(IPacket)) != null);
    var call = Expression.Call(method,
                               Expression.Convert(sessionParameter, sessionParamInfo.ParameterType),
                               Expression.Convert(messageParameter, messageParamInfo.ParameterType));
    var lambda = Expression.Lambda<MessageHandlerDelegate>(call, sessionParameter, messageParameter);
    return lambda.Compile();
  }


  public IPacket GetMessage(int messageId) {
    return MessageFactories.TryGetValue(messageId, out var factory)
             ? factory.Invoke()
             : null;
  }


  public bool GetOpcode(IPacket message, out int messageId) {
    return OpCodes.TryGetValue(message.GetType(), out messageId);
  }


  public bool TryGetMessageHandler(int messageId, out MessageHandlerDelegate handler) {
    return MessageHandlers.TryGetValue(messageId, out handler);
  }
}