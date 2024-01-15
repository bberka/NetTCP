

using System.Net;
using System.Reflection;
using NetTCP.Attributes;
using NetTCP.Example.Server.Abstract;
using NetTCP.Example.Server.Concrete;
using NetTCP.Example.Shared;
using NetTCP.Example.Shared.Network.Packets.Server;
using NetTCP.Server;



var builder = NetTcpServerBuilder.Create();
builder.RegisterPacketsFromAssembly(typeof(OpCodes).Assembly);

builder.RegisterSingleton<IServerInfoMgr,ServerInfoMgr>();

var server = builder.Build("127.0.0.1", 8080);


server.ServerStarted += (sender, args) => { Console.WriteLine("Server started"); };

server.ServerStopped += (sender, args) => { Console.WriteLine("Server stopped"); };

server.ServerError += (sender, args) => { Console.WriteLine($"Server error: {args.Exception}"); };

server.ClientConnected += (sender, args) => { Console.WriteLine($"New client connected: {args.Connection.RemoteIpAddress.ToString()}"); };

server.ClientDisconnected += (sender, args) => { Console.WriteLine($"Client disconnected: {args.Connection.RemoteIpAddress.ToString()}"); };

server.UnknownPacketReceived += (sender, args) => { Console.WriteLine($"Unknown packet received from {args.Connection.RemoteIpAddress.ToString()}"); };

server.UnknownPacketSendAttempted += (sender, args) => { Console.WriteLine($"Unknown packet send attempted to {args.Connection.RemoteIpAddress.ToString()}"); };

server.MessageHandlerNotFound += (sender, args) => { Console.WriteLine($"Message handler not found for {args.Packet.MessageId} from {args.Connection.RemoteIpAddress.ToString()}"); };

server.PacketQueued += (sender, args) => { Console.WriteLine($"Packet queued for {args.Connection.RemoteIpAddress.ToString()} with message id {args.MessageId}"); };

server.PacketReceived += (sender, args) => { Console.WriteLine($"Packet received from {args.Connection.RemoteIpAddress.ToString()} with message id {args.MessageId}"); };

server.StartServerAsync().GetAwaiter().GetResult();