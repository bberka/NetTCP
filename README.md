# NetTCP 
Simple TCPListener and TCPClient wrapper for .NET 6+. Provides easy access to events and messages/op codes and handlers

Simple use and support for Autofac DI container and scopes for each message handler instance
## Warning
This is a work in progress and not ready for production use. Use at your own risk

If you want to fork and improve or implement missing features, feel free to do so

## Features
- [x] TCPListener wrapper as NetTcpServer with NetTcpConnection for each client
- [x] TCPClient wrapper as NetTcpClient
- [x] Packet handler with opcodes
- [x] Autofac DI container for each message handler
- [x] Packet pool for each server and client
- [x] Packet registration from assembly
- [x] Fully implmented events for server

## Todo
- Fully implment events for client
- DI Scope for each connection aside from message handler instance scope
- Better logging (Serilog maybe)
- Better error handling
- Improve performance and cleanup
- Allowing contructor injection for message handlers
- Better documentation
- Better abstraction for server and client to be overriden for custom implementations

## Usage
- Install NuGet package
- Create 3 projects (Shared, Client, Server)
- Add reference to Shared project in Client and Server
- Define opcodes and packets in Shared project
- Register packets in Client and Server project from Shared project assembly
- Define packet handlers in Client and Server project
- Build and run

## Things to note
- Each Packet class must implement IPacket interface as well ass [PacketAttribute] otherwise it will not register
- Each Packet Handler method must implement [PacketHandlerAttribute(**OPCODE**)] otherwise it will not register
- Defining 2 Message Handlers for same op code will throw an error (Dictionary key error)
- Defining 2 Packets with same op code will throw an error (Dictionary key error)
- Assembly types will be merged meaning when you use RegisterPacketsFromAssembly you can use as many assembly as you want


## Examples
For more details see the example project

#### Shared (Client and Server)
Define enum for opcodes
```csharp
public enum OpCodes
{
  CMPing = 1000,
  SMPong
}
```

Define a client packet
```csharp
[Packet(OpCodes.CMPing)]
public sealed class CmPing : IPacket
{
  public long Timestamp { get; set; } = 0;
}
```

Define a server packet
```csharp
[Packet(OpCodes.SMPong)]
public sealed class SmPong : IPacket
{
  public long Timestamp { get; set; } = 0;
}
```


#### Client 
Program.cs
```csharp
// Create a new client builder
var builder = NetTcpClientBuilder.Create();

//Register packet from another assembly
builder.RegisterPacketsFromAssembly(typeof(OpCodes).Assembly);

//Build NetTcpClient
var client = builder.Build("127.0.0.1", 8080);

//Register events here (not implemented yet)
 
//Connect to server
client.Connect();

//Block thread until client is disconnected and send ping every 5 seconds
while (client.CanProcess) {
  Thread.Sleep(5000);
  client.EnqueuePacketSend(new CmPing {
    Timestamp = 123123
  });
}

Console.WriteLine("Client disconnected");
```
Client Packet Handler
```csharp
public static class PongHandler
{
  [PacketHandler(OpCodes.SMPong)]
  public static void HandlePing(NetTcpClient client, SmPong request, ILifetimeScope scope) {
    Console.WriteLine($"[NetTCP - Client] Pong received from server with timestamp {request.Timestamp}.");
    Thread.Sleep(1000); //dont do this
    client.EnqueuePacketSend(new CmPing {
      Timestamp = request.Timestamp + 1
    });
  }
}
```



#### Server
Program.cs
```csharp
// Create a new server builder
var builder = NetTcpServerBuilder.Create();

//Register packet from another assembly
//Entry assembly is always added to the packet pool
builder.RegisterPacketsFromAssembly(typeof(OpCodes).Assembly);

//Register a service to DI container
builder.RegisterSingleton<IServerInfoMgr, ServerInfoMgr>();

//Register a handler for a packet and builds DI container
var server = builder.Build("127.0.0.1", 8080);

//Register events
server.ServerStarted += (sender, args) => { Console.WriteLine("Server started on " + server.IpAddress + ":" + server.Port); };

//Start listening also blocks the thread
server.StartServerAsync().GetAwaiter().GetResult();
```

Server Packet Handler
```csharp
public static class PingHandler
{
  [PacketHandler(OpCodes.CMPing)]
  public static void HandlePing(NetTcpConnection connection, CmPing request, ILifetimeScope scope) {
    var serverInfoMgr = scope.Resolve<IServerInfoMgr>();
    Console.WriteLine($"[NetTCP - Server - {serverInfoMgr.Name}] Ping received from {connection.RemoteIpAddress} with timestamp {request.Timestamp}.");
    Thread.Sleep(1000); //dont do this
    connection.EnqueuePacketSend(new SmPong {
      Timestamp = request.Timestamp + 1
    });
  }
}
```