using Autofac;
using NetTCP;
using NetTCP.Example.Server;
using NetTCP.Example.Server.Abstract;
using NetTCP.Example.Server.Concrete;
using NetTCP.Example.Shared;
using NetTCP.Server;

ExampleServer.This.StartAndWaitServer();

Console.WriteLine("Server app stopped");