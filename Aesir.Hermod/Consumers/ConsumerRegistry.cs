using Aesir.Hermod.Consumers.Interfaces;
using Aesir.Hermod.Exceptions;
using Aesir.Hermod.Extensions;
using Aesir.Hermod.Messages.Interfaces;
using Microsoft.Extensions.Logging;

namespace Aesir.Hermod.Consumers;

internal class ConsumerRegistry : IConsumerRegistry
{
    private readonly List<Type> _consumers = new();
    private readonly ILogger<ConsumerRegistry> _logger;

    public ConsumerRegistry(IServiceProvider sp)
    {
        _logger = sp.GetLogger<ConsumerRegistry>();
    }

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
        if (_consumers.Any(x => x == type))
        {
            _logger.LogDebug("A consumer for type {type} has already been registered.", type.FullName);
            return false;
        }
        _logger.LogDebug("Registred a consumer for type {type}.", type.FullName);
        _consumers.Add(type);
        return true;
    }

    internal Type? Find(string message) => _consumers.Where(x => GetParamType(x)?.Name == message).FirstOrDefault();

    private static Type? GetParamType(Type method) => method.GetInterfaces().FirstOrDefault()?.GenericTypeArguments.FirstOrDefault();

    internal bool TryGet<T>(out Type? consumer) where T : class
    {
        consumer = null;
        var type = typeof(T);

        if (!typeof(T).ImplementsGenericInterface(typeof(IConsumer<>)))
            throw new MessageReceiveException($"The provided type {type.FullName} does not implement IConsumer.");

        consumer = _consumers.Where(c => c == type).FirstOrDefault();
        return consumer != null;
    }
}
