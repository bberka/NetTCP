using System.Net;
using System.Reflection;
using Autofac;
using Autofac.Core;
using NetTCP.Abstract;

namespace NetTCP.Client;

public class NetTcpClientBuilder
{
  private readonly ContainerBuilder _containerBuilder;
  private readonly NetTcpClientPacketContainer _packetContainer;

  private ISerializer _serializer = new JsonSerializer();

  private NetTcpClientBuilder() {
    _containerBuilder = new ContainerBuilder();
    _packetContainer = new NetTcpClientPacketContainer();
  }


  /// <summary>
  ///   Creates a new instance of <see cref="NetTcpServerBuilder" /> with default settings.
  ///   It will register all packets from the entry assembly.
  /// </summary>
  /// <returns></returns>
  public static NetTcpClientBuilder Create() {
    return new NetTcpClientBuilder();
  }

  public NetTcpClientBuilder UseSerializer(ISerializer serializer) {
    _serializer = serializer;
    return this;
  }

  public NetTcpClientBuilder RegisterSingleton<TService, TImplementation>() where TImplementation : TService {
    _containerBuilder.RegisterType<TImplementation>().As<TService>().SingleInstance();
    return this;
  }

  public NetTcpClientBuilder RegisterSingleton<TService>(TService instance) where TService : class {
    _containerBuilder.RegisterInstance(instance).As<TService>().SingleInstance();
    return this;
  }

  public NetTcpClientBuilder RegisterSingleton<TService>(Func<IComponentContext, TService> factory) where TService : class {
    _containerBuilder.Register(factory).As<TService>().SingleInstance();
    return this;
  }

  public NetTcpClientBuilder RegisterSingleton<TService>(Func<IComponentContext, IEnumerable<Parameter>, TService> factory) where TService : class {
    _containerBuilder.Register(factory).As<TService>().SingleInstance();
    return this;
  }

  public NetTcpClientBuilder RegisterSingleton<TService>(Func<IComponentContext, IEnumerable<Parameter>, Task<TService>> factory) where TService : class {
    _containerBuilder.Register(factory).As<TService>().SingleInstance();
    return this;
  }

  public NetTcpClientBuilder RegisterScoped<TService, TImplementation>() where TImplementation : TService {
    _containerBuilder.RegisterType<TImplementation>().As<TService>().InstancePerLifetimeScope();
    return this;
  }

  public NetTcpClientBuilder RegisterScoped<TService>(Func<IComponentContext, TService> factory) where TService : class {
    _containerBuilder.Register(factory).As<TService>().InstancePerLifetimeScope();
    return this;
  }

  public NetTcpClientBuilder RegisterScoped<TService>(Func<IComponentContext, IEnumerable<Parameter>, TService> factory) where TService : class {
    _containerBuilder.Register(factory).As<TService>().InstancePerLifetimeScope();
    return this;
  }

  public NetTcpClientBuilder RegisterScoped<TService>(Func<IComponentContext, IEnumerable<Parameter>, Task<TService>> factory) where TService : class {
    _containerBuilder.Register(factory).As<TService>().InstancePerLifetimeScope();
    return this;
  }

  public NetTcpClientBuilder RegisterTransient<TService, TImplementation>() where TImplementation : TService {
    _containerBuilder.RegisterType<TImplementation>().As<TService>().InstancePerDependency();
    return this;
  }

  public NetTcpClientBuilder RegisterTransient<TService>(Func<IComponentContext, TService> factory) where TService : class {
    _containerBuilder.Register(factory).As<TService>().InstancePerDependency();
    return this;
  }

  public NetTcpClientBuilder RegisterTransient<TService>(Func<IComponentContext, IEnumerable<Parameter>, TService> factory) where TService : class {
    _containerBuilder.Register(factory).As<TService>().InstancePerDependency();
    return this;
  }

  public NetTcpClientBuilder RegisterTransient<TService>(Func<IComponentContext, IEnumerable<Parameter>, Task<TService>> factory) where TService : class {
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
  public NetTcpClientBuilder RegisterPacketsFromAssembly(Assembly assembly) {
    _packetContainer.Register(assembly);
    return this;
  }

  public NetTcpClient Build(string host, ushort port) {
    var parseIp = IPAddress.TryParse(host, out var ipAddress);
    if (parseIp == false)
      throw new ArgumentException("Invalid ip address: " + host, nameof(host));
    var isValidPort = NetTcpTools.IsValidPort(port);
    if (isValidPort == false)
      throw new ArgumentException("Invalid port: " + port, nameof(port));
    var container = _containerBuilder.Build();
    _packetContainer.InitializeBuild(container);
    var server = new NetTcpClient(host, port, _packetContainer, _serializer);
    return server;
  }
}