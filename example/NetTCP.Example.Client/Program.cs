using NetTCP;
using NetTCP.Client;
using NetTCP.Example.Client;
using NetTCP.Example.Shared;
using NetTCP.Example.Shared.Network.Message.Client;

Task.Run((() => {
             Thread.Sleep(3000);
             ExampleClient.This.Client.Disconnect(Reason.ClientDisconnected);
           }));
while (ExampleClient.This.Client.CanRead) {
  Thread.Sleep(100);
  // ExampleClient.This.Client.Disconnect(Reason.Unknown);
  ExampleClient.This.Client.EnqueuePacketSend(new CmPing());
  Console.WriteLine("Sent ping");
}

Console.WriteLine("Client disconnected");