using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using NetTCP.Abstract;
using NetTCP.Attributes;

namespace NetTCP.Server;

public delegate void MessageHandlerDelegate(EasTcpConnection session, IPacketReadable message);

public class ServerPacketTable
{
  private delegate IPacketReadable MessageFactoryDelegate();

  private ServerPacketTable() { }

  public static ServerPacketTable This {
    get {
      _instance ??= new();
      return _instance;
    }
  }

  private static ServerPacketTable? _instance;

  private ImmutableDictionary<int, MessageFactoryDelegate> _clientMessageFactories;
  private ImmutableDictionary<Type, int> _serverMessageOpcodes;

  private ImmutableDictionary<int, MessageHandlerDelegate> _clientMessageHandlers;

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

    _clientMessageFactories = messageFactories.ToImmutableDictionary();
    _serverMessageOpcodes = messageOpcodes.ToImmutableDictionary();
 
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

        var sessionParameter = Expression.Parameter(typeof(EasTcpConnection));
        var messageParameter = Expression.Parameter(typeof(IPacketReadable));

        var parameterInfo = method.GetParameters();

        if (method.IsStatic) {
          #region Debug

          Debug.Assert(parameterInfo.Length == 2);
          Debug.Assert(typeof(EasTcpConnection).IsAssignableFrom(parameterInfo[0].ParameterType));
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
          Debug.Assert(typeof(EasTcpConnection).IsAssignableFrom(type));
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

  internal MessageHandlerDelegate GetMessageHandler(int messageId) {
    return _clientMessageHandlers.TryGetValue(messageId, out MessageHandlerDelegate handler)
             ? handler
             : null;
  }
  
}