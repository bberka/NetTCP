using Autofac;
using NetTCP.Example.Server.Abstract;
using NetTCP.Example.Server.Concrete;
using NetTCP.Example.Shared;
using NetTCP.Server;

namespace NetTCP.Example.Server;

public class ExampleClient
{

  public NetTcpServer Server { get; private set; }
  private ExampleClient() {
    
    var containerBuilder = new ContainerBuilder();

    containerBuilder.RegisterType<ServerInfoMgr>().As<IServerInfoMgr>().SingleInstance();

    var container = containerBuilder.Build();
    Server = new NetTcpServer("127.0.0.1", 8080,container,new [] {
      typeof(OpCodes).Assembly
    });

    Server.ServerStarted += (sender, args) => { Console.WriteLine("Server started on " + Server.IpAddress + ":" + Server.Port); };
    Server.ServerStopped += (sender, args) => { Console.WriteLine("Server stopped"); };
    Server.ServerError += (sender, args) => { Console.WriteLine($"Server error: {args.Exception}"); };
    Server.ClientConnected += (sender, args) => { Console.WriteLine($"New client connected: {args.Connection.RemoteIpAddress}"); };
    Server.ClientDisconnected += (sender, args) => {
      Console.WriteLine($"Client disconnected: {args.Connection.RemoteIpAddress}");
    };
    Server.UnknownPacketReceived += (sender, args) => { Console.WriteLine($"Unknown packet received from {args.Connection.RemoteIpAddress}"); };
    Server.UnknownPacketSendAttempted += (sender, args) => { Console.WriteLine($"Unknown packet send attempted to {args.Connection.RemoteIpAddress}"); };
    Server.MessageHandlerNotFound += (sender, args) => { Console.WriteLine($"Message handler not found for {args.Packet.MessageId} from {args.Connection.RemoteIpAddress}"); };
    Server.PacketQueued += (sender, args) => { Console.WriteLine($"Packet queued for {args.Connection.RemoteIpAddress} with message id {args.OpCode}"); };
    Server.PacketReceived += (sender, args) => { Console.WriteLine($"Packet received from {args.Connection.RemoteIpAddress} with message id {args.MessageId}"); };
    Server.HandlerError += (sender, args) => { Console.WriteLine($"Handler error {args.Connection.RemoteIpAddress} with message id {args.Exception.Message}"); };

  }

  public static ExampleClient This {
    get {
      _instance ??= new();
      return _instance;
    }
  }

  private static ExampleClient? _instance;

  public void StartAndWaitServer() {
    Server.StartServerAsync().GetAwaiter().GetResult();
  }
  
}