using NetTCP.Client;
using NetTCP.Example.Shared;
using NetTCP.Example.Shared.Network.Message.Client;

var builder = NetTcpClientBuilder.Create();

builder.RegisterPacketsFromAssembly(typeof(OpCodes).Assembly);


var client = builder.Build("127.0.0.1", 8080);

client.ClientConnected += (sender, args) => {
  Console.WriteLine("Client connected");
};

client.ClientDisconnected += (sender, args) => {
  Console.WriteLine("Client disconnected");
};

client.PacketReceived += (sender, args) => {
  Console.WriteLine($"Received packet {args.MessageId}");
};

client.PacketQueued += (sender, args) => {
  Console.WriteLine($"Queued packet {args.MessageId}");
};

client.UnknownPacketReceived += (sender, args) => {
  Console.WriteLine($"Unknown packet received {args.MessageId}");
};

client.UnknownPacketSendAttempted += (sender, args) => {
  Console.WriteLine($"Unknown packet send attempted {args.Message.ToString()}");
};

client.MessageHandlerNotFound += (sender, args) => {
  Console.WriteLine($"Message handler not found {args.Packet.ToString()}");
};


client.ConnectWithRetry(3,1000);

while (client.CanProcess) {
  Thread.Sleep(5000);
  client.EnqueuePacketSend(new CmPing {
    Timestamp = 123123
  });
}

Console.WriteLine("Client disconnected");