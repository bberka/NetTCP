using System.Net;
using System.Reflection;
using Autofac;
using Autofac.Core;
using NetTCP.Abstract;

namespace NetTCP.Server;

public class NetTcpServerBuilder
{
  private readonly ContainerBuilder _containerBuilder;
  private readonly NetTcpServerPacketContainer _packetContainer;
  private ISerializer _serializer = new JsonSerializer();

  private NetTcpServerBuilder() {
    _containerBuilder = new ContainerBuilder();
    _packetContainer = new NetTcpServerPacketContainer();
  }


  /// <summary>
  ///   Creates a new instance of <see cref="NetTcpServerBuilder" /> with default settings.
  ///   It will register all packets from the entry assembly.
  /// </summary>
  /// <returns></returns>
  public static NetTcpServerBuilder Create() {
    var builder = new NetTcpServerBuilder();
    return builder;
  }

  public NetTcpServerBuilder UseSerializer(ISerializer serializer) {
    _serializer = serializer;
    return this;
  }

  public NetTcpServerBuilder RegisterSingleton<TService, TImplementation>() where TImplementation : TService {
    _containerBuilder.RegisterType<TImplementation>().As<TService>().SingleInstance();
    return this;
  }

  public NetTcpServerBuilder RegisterSingleton<TService>(TService instance) where TService : class {
    _containerBuilder.RegisterInstance(instance).As<TService>().SingleInstance();
    return this;
  }

  public NetTcpServerBuilder RegisterSingleton<TService>(Func<IComponentContext, TService> factory) where TService : class {
    _containerBuilder.Register(factory).As<TService>().SingleInstance();
    return this;
  }

  public NetTcpServerBuilder RegisterSingleton<TService>(Func<IComponentContext, IEnumerable<Parameter>, TService> factory) where TService : class {
    _containerBuilder.Register(factory).As<TService>().SingleInstance();
    return this;
  }

  public NetTcpServerBuilder RegisterSingleton<TService>(Func<IComponentContext, IEnumerable<Parameter>, Task<TService>> factory) where TService : class {
    _containerBuilder.Register(factory).As<TService>().SingleInstance();
    return this;
  }

  public NetTcpServerBuilder RegisterScoped<TService, TImplementation>() where TImplementation : TService {
    _containerBuilder.RegisterType<TImplementation>().As<TService>().InstancePerLifetimeScope();
    return this;
  }

  public NetTcpServerBuilder RegisterScoped<TService>(Func<IComponentContext, TService> factory) where TService : class {
    _containerBuilder.Register(factory).As<TService>().InstancePerLifetimeScope();
    return this;
  }

  public NetTcpServerBuilder RegisterScoped<TService>(Func<IComponentContext, IEnumerable<Parameter>, TService> factory) where TService : class {
    _containerBuilder.Register(factory).As<TService>().InstancePerLifetimeScope();
    return this;
  }

  public NetTcpServerBuilder RegisterScoped<TService>(Func<IComponentContext, IEnumerable<Parameter>, Task<TService>> factory) where TService : class {
    _containerBuilder.Register(factory).As<TService>().InstancePerLifetimeScope();
    return this;
  }

  public NetTcpServerBuilder RegisterTransient<TService, TImplementation>() where TImplementation : TService {
    _containerBuilder.RegisterType<TImplementation>().As<TService>().InstancePerDependency();
    return this;
  }

  public NetTcpServerBuilder RegisterTransient<TService>(Func<IComponentContext, TService> factory) where TService : class {
    _containerBuilder.Register(factory).As<TService>().InstancePerDependency();
    return this;
  }

  public NetTcpServerBuilder RegisterTransient<TService>(Func<IComponentContext, IEnumerable<Parameter>, TService> factory) where TService : class {
    _containerBuilder.Register(factory).As<TService>().InstancePerDependency();
    return this;
  }

  public NetTcpServerBuilder RegisterTransient<TService>(Func<IComponentContext, IEnumerable<Parameter>, Task<TService>> factory) where TService : class {
    _containerBuilder.Register(factory).As<TService>().InstancePerDependency();
    return this;
  }

  /// <summary>
  ///   This method can only be called once.
  ///   It will register all packets and packet handlers from the given assembly.
  ///   You do not need to provide an assembly if you want to register all packets from the entry assembly.
  ///   If you provide an assembly that is different than entry assembly it will register all packets from the entry assembly
  ///   and the given assembly.
  /// </summary>
  /// <param name="assembly"></param>
  /// <returns></returns>
  public NetTcpServerBuilder RegisterPacketsFromAssembly(Assembly assembly) {
    _packetContainer.Register(assembly);
    return this;
  }

  public NetTcpServer Build(string ip, ushort port) {
    var parseIp = IPAddress.TryParse(ip, out var ipAddress);
    if (parseIp == false)
      throw new ArgumentException("Invalid ip address: " + ip, nameof(ip));
    var isValidPort = NetTcpTools.IsValidPort(port);
    if (isValidPort == false)
      throw new ArgumentException("Invalid port: " + port, nameof(port));
    var container = _containerBuilder.Build();
    _packetContainer.InitializeBuild(container);
    var server = new NetTcpServer(ipAddress, port, _packetContainer, _serializer);
    return server;
  }
}