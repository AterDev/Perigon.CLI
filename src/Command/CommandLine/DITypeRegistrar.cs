namespace CommandLine;

using System;
using Spectre.Console.Cli;

public sealed class DITypeRegistrar(IServiceProvider serviceProvider) : ITypeRegistrar
{
    public void Register(Type service, Type implementation) { }

    public void RegisterInstance(Type service, object implementation) { }

    public void RegisterLazy(Type service, Func<object> factory) { }

    public ITypeResolver Build()
    {
        return new DITypeResolver(serviceProvider);
    }
}

public sealed class DITypeResolver(IServiceProvider serviceProvider) : ITypeResolver, IDisposable
{
    public object? Resolve(Type? type)
    {
        return type == null ? null : serviceProvider.GetService(type);
    }

    public void Dispose()
    {
        if (serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
