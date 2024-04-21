using Autofac;
using NetTCP.Example.Server.Abstract;
using NetTCP.Example.Server.Concrete;
using NetTCP.Example.Shared;
using NetTCP.Example.Shared.Network.Message.Common;
using NetTCP.Server;

namespace NetTCP.Example.Server;

public class ExampleServer
{
  public NetTcpServer Server { get; private set; }

  private ExampleServer() {
    var containerBuilder = new ContainerBuilder();

    containerBuilder.RegisterType<ServerInfoMgr>().As<IServerInfoMgr>().SingleInstance();

    var container = containerBuilder.Build();
    Server = new NetTcpServer("127.0.0.1",
                              8080,
                              container,
                              new[] {
                                typeof(OpCodes).Assembly
                              });

    Server.ServerStarted += (sender, args) => { Console.WriteLine($"Server started on {Server.IpAddress}:{Server.Port}"); };
    Server.ServerStopped += (sender, args) => { Console.WriteLine("Server stopped"); };
    Server.ServerError += (sender, args) => { Console.WriteLine($"Server error: {args.Exception}"); };
    Server.ClientConnected += (sender, args) => {
      // args.Session.EnqueuePacketSend(new VersionInformation());
      Console.WriteLine($"New client connected: {args.Session.RemoteIpAddress}");
    };
    Server.ClientDisconnected += (sender, args) => {
      Console.WriteLine($"Session disconnected: {args.Session.RemoteIpAddress}");
    };
    Server.UnknownPacketReceived += (sender, args) => { Console.WriteLine($"Unknown packet received with id {args.MessageId}"); };
    Server.UnknownPacketSendAttempted += (sender, args) => { Console.WriteLine($"Unknown packet send attempted to {args.Session.RemoteIpAddress}"); };
    Server.MessageHandlerNotFound += (sender, args) => { Console.WriteLine($"Message handler not found for {args.ProcessedIncomingPacket.MessageId} from {args.Session.RemoteIpAddress}"); };
    Server.PacketQueued += (sender, args) => { Console.WriteLine($"Packet queued for {args.Session.RemoteIpAddress} with message id {args.OpCode}"); };
    Server.PacketReceived += (sender, args) => { Console.WriteLine($"Packet received from {args.Session.RemoteIpAddress} with message id {args.MessageId}"); };
    Server.HandlerError += (sender, args) => { Console.WriteLine($"Handler error {args.Session.RemoteIpAddress} with message id {args.Exception.Message}"); };
  }

  public static ExampleServer This {
    get {
      _instance ??= new();
      return _instance;
    }
  }

  private static ExampleServer? _instance;

  public void StartAndWaitServer() {
    Server.StartServerAsync().GetAwaiter().GetResult();
  }
}