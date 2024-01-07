using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using EasTCP.Abstract;
using EasTCP.Attributes;

namespace EasTCP.Client;

public delegate void MessageHandlerDelegate(EasTcpClient session, IPacketReadable message);

public class ClientPacketTable
{
  private delegate IPacketReadable MessageFactoryDelegate();

  private ClientPacketTable() { }

  public static ClientPacketTable This {
    get {
      _instance ??= new();
      return _instance;
    }
  }

  private static ClientPacketTable? _instance;

  private ImmutableDictionary<int, MessageFactoryDelegate> _serverMessageFactories;
  private ImmutableDictionary<Type, int> _clientMessageOpcodes;

  private ImmutableDictionary<int, MessageHandlerDelegate> _serverMessageHandlers;

  /// <summary>
  /// Register all message handlers and messages from an assembly
  /// </summary>
  /// <param name="assembly"></param>
  public void Register(Assembly assembly) {
    var types = assembly.GetTypes();
    RegisterMessageHandlers(types);
    RegisterMessages(types);
  }
  
  public void Register(Assembly[] assembly) {
    var types = assembly.SelectMany(x => x.GetTypes()).ToArray();
    RegisterMessageHandlers(types);
    RegisterMessages(types);
  }

  private void RegisterMessages(Type[] types) {
    // Assembly.GetExecutingAssembly()
    //         .GetTypes()
    //         .Concat(Assembly.GetEntryAssembly().GetTypes());
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

    _serverMessageFactories = messageFactories.ToImmutableDictionary();
    _clientMessageOpcodes = messageOpcodes.ToImmutableDictionary();
//TODO LOG    
  }

  private void RegisterMessageHandlers(Type[] types) {
    var messageHandlers = new Dictionary<int, MessageHandlerDelegate>();

    foreach (var type in types) {
      foreach (var method in type.GetMethods()) {
        if (method.DeclaringType != type)
          continue;

        var attribute = method.GetCustomAttribute<PacketHandlerAttribute>();
        if (attribute == null)
          continue;

        var sessionParameter = Expression.Parameter(typeof(EasTcpClient));
        var messageParameter = Expression.Parameter(typeof(IPacketReadable));

        var parameterInfo = method.GetParameters();

        if (method.IsStatic) {
          #region Debug

          Debug.Assert(parameterInfo.Length == 2);
          Debug.Assert(typeof(EasTcpClient).IsAssignableFrom(parameterInfo[0].ParameterType));
          Debug.Assert(typeof(IPacketReadable).IsAssignableFrom(parameterInfo[1].ParameterType));

          #endregion

          var call = Expression.Call(method,
                                     Expression.Convert(sessionParameter, parameterInfo[0].ParameterType),
                                     Expression.Convert(messageParameter, parameterInfo[1].ParameterType));

          var lambda =
            Expression.Lambda<MessageHandlerDelegate>(call, sessionParameter, messageParameter);

          messageHandlers.Add(attribute.MessageId, lambda.Compile());
        }
        else {
          #region Debug

          Debug.Assert(parameterInfo.Length == 1);
          Debug.Assert(typeof(EasTcpClient).IsAssignableFrom(type));
          Debug.Assert(typeof(IPacketReadable).IsAssignableFrom(parameterInfo[0].ParameterType));

          #endregion

          var call = Expression.Call(
                                     Expression.Convert(sessionParameter, type),
                                     method,
                                     Expression.Convert(messageParameter, parameterInfo[0].ParameterType));

          var lambda =
            Expression.Lambda<MessageHandlerDelegate>(call, sessionParameter, messageParameter);

          messageHandlers.Add(attribute.MessageId, lambda.Compile());
        }
      }
    }

    _serverMessageHandlers = messageHandlers.ToImmutableDictionary();
////TODO LOG    
  }

  internal IPacketReadable GetMessage(int messageId) {
    return _serverMessageFactories.TryGetValue(messageId, out MessageFactoryDelegate factory)
             ? factory.Invoke()
             : null;
  }

  internal bool GetOpcode(IPacketWriteable message, out int messageId) {
    return _clientMessageOpcodes.TryGetValue(message.GetType(), out messageId);
  }

  internal MessageHandlerDelegate GetMessageHandler(int messageId) {
    return _serverMessageHandlers.TryGetValue(messageId, out MessageHandlerDelegate handler)
             ? handler
             : null;
  }
  
}