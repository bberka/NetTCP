using NetTCP;
using NetTCP.Client;
using NetTCP.Example.Client;
using NetTCP.Example.Shared;
using NetTCP.Example.Shared.Network.Message.Client;


ExampleClient.This.Client.EnqueuePacketSend(new CmPing());
// while (ExampleClient.This.Client.CanRead) {
//   ExampleClient.This.Client.EnqueuePacketSend(new CmPing());
//   Thread.Sleep(5000);
// }

Console.ReadLine();
Console.WriteLine("Session disconnected");