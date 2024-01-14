using System.Net.NetworkInformation;
using System.Reflection;
using NetTCP.Client;
using NetTCP.Example.Shared;
using NetTCP.Example.Shared.Network.Packets.Client;



var entry = Assembly.GetEntryAssembly();
var shared = typeof(OpCodes).Assembly;
ClientPacketTable.This.Register(new[] {
  entry,
  shared
});

Thread.Sleep(3000);
var client = new NetTcpClient("127.0.0.1", 8080);
client.Connect();

//BLOCK THREAD
while (client.CanProcess) {
  Thread.Sleep(5000);
  client.EnqueuePacketSend(new CMPing() {
    Timestamp = 123123
  });
}

Console.WriteLine("Client disconnected");