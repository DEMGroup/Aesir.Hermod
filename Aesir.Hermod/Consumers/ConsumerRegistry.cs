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
        if (!type.ImplementsGenericInterface(typeof(IConsumer<>))) return false;
        if (_consumers.Any(x => x == type)) return false;

        _consumers.Add(type);
        return true;
    }

    internal Type? Find(string consumer) => _consumers.Where(x => GetParamType(x)?.Name == consumer).FirstOrDefault();

    private static Type? GetParamType(Type method) => method.GetInterfaces().FirstOrDefault()?.GenericTypeArguments.FirstOrDefault();

    internal bool TryGet<T>(out Type? consumer) where T : class
    {
        consumer = null;
        if (!typeof(T).ImplementsGenericInterface(typeof(IConsumer<>))) return false;

        var type = typeof(T);
        consumer = _consumers.Where(c => c == type).FirstOrDefault();
        return consumer != null;
    }
}
