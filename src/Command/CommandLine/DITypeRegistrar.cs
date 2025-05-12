namespace CommandLine;
using System;
using Spectre.Console.Cli;

public sealed class DITypeRegistrar : ITypeRegistrar
{
    private readonly IServiceProvider _serviceProvider;

    public DITypeRegistrar(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Register(Type service, Type implementation)
    {
    }

    public void RegisterInstance(Type service, object implementation)
    {
    }

    public void RegisterLazy(Type service, Func<object> factory)
    {
    }

    public ITypeResolver Build()
    {
        return new DITypeResolver(_serviceProvider);
    }
}

public sealed class DITypeResolver : ITypeResolver, IDisposable
{
    private readonly IServiceProvider _serviceProvider;

    public DITypeResolver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public object? Resolve(Type? type)
    {
        return type == null ? null : _serviceProvider.GetService(type);
    }

    public void Dispose()
    {
        if (_serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
