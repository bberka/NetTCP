using NetTCP.Client;
using NetTCP.Example.Shared;
using NetTCP.Example.Shared.Network.Message.Client;

var builder = NetTcpClientBuilder.Create();
builder.RegisterPacketsFromAssembly(typeof(OpCodes).Assembly);


var client = builder.Build("127.0.0.1", 8080);

//REGISTER EVENTS

client.Connect();

while (client.CanProcess) {
  Thread.Sleep(5000);
  client.EnqueuePacketSend(new CmPing {
    Timestamp = 123123
  });
}

Console.WriteLine("Client disconnected");