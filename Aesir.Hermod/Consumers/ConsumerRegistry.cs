using Aesir.Hermod.Consumers.Interfaces;
using Aesir.Hermod.Exceptions;
using Aesir.Hermod.Extensions;

namespace Aesir.Hermod.Consumers;

internal class ConsumerRegistry : IConsumerRegistry
{
    private readonly List<Type> _consumers = new();

    public void RegisterConsumer(Type type)
    {
        if (!type.ImplementsGenericInterface(typeof(IConsumer<>)))
            throw new ConfigurationException($"Consumer must implement interface {typeof(IConsumer<>).Name}");

        var success = TryAdd(type);
        if (!success)
            throw new ConfigurationException($"Error when adding type {type.Name}");
    }

    internal bool TryAdd(Type type)
    {
        if (_consumers.Any(x => x == type)) return false;
        _consumers.Add(type);
        return true;
    }

    internal Type? Find(string message) => _consumers.Find(x => GetParamType(x)?.Name == message);

    private static Type? GetParamType(Type method) => method.GetInterfaces().FirstOrDefault()?.GenericTypeArguments.FirstOrDefault();
}
