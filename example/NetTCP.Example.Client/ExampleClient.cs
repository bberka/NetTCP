using Autofac;
using NetTCP.Client;
using NetTCP.Example.Shared;
using NetTCP.Example.Shared.Network.Message.Client;
using NetTCP.Example.Shared.Network.Message.Common;

namespace NetTCP.Example.Client;

public class ExampleClient
{
  public NetTcpClient Client { get; private set; }

  private ExampleClient() {
    var containerBuilder = new ContainerBuilder();

    var container = containerBuilder.Build();

    Client = new NetTcpClient("127.0.0.1", 8080, container, new [] {
      typeof(OpCodes).Assembly
    });

    Client.ClientConnected += (sender, args) => {
      Console.WriteLine("Session connected ");
      args.Session.EnqueuePacketSend(new VersionInformation());
    };
    Client.ClientDisconnected += (sender, args) => { Console.WriteLine($"Session disconnected reason: {args.NetTcpErrorReason}"); };
    Client.PacketReceived += (sender, args) => { Console.WriteLine($"Received packet {args.MessageId}"); };
    Client.PacketQueued += (sender, args) => { Console.WriteLine($"Queued packet {args.OpCode}"); };
    Client.UnknownPacketReceived += (sender, args) => { Console.WriteLine($"Unknown packet received {args.MessageId}"); };
    Client.UnknownPacketSendAttempted += (sender, args) => { Console.WriteLine($"Unknown packet send attempted {args.Message.ToString()}"); };
    Client.MessageHandlerNotFound += (sender, args) => { Console.WriteLine($"Message handler not found {args.ProcessedIncomingPacket.ToString()}"); };
    Client.HandlerError += (sender, args) => { Console.WriteLine($"Handler error {args.Exception.Message.ToString()}"); };
    Client.Connect();
  }

  public static ExampleClient This {
    get {
      _instance ??= new();
      return _instance;
    }
  }

  private static ExampleClient? _instance;
}