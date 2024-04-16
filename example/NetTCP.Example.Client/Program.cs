using NetTCP;
using NetTCP.Client;
using NetTCP.Example.Client;
using NetTCP.Example.Shared;
using NetTCP.Example.Shared.Network.Message.Client;


while (ExampleClient.This.Client.CanRead) {
  ExampleClient.This.Client.EnqueuePacketSend(new CmPing());
}

Console.WriteLine("Client disconnected");