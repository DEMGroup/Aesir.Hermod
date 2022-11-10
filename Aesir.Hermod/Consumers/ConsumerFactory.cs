using Aesir.Hermod.Consumers.Interfaces;
using Aesir.Hermod.Exceptions;
using Aesir.Hermod.Extensions;

namespace Aesir.Hermod.Consumers;

public class ConsumerFactory : IConsumerFactory
{
    internal List<Type> Consumers { get; } = new();
    private readonly string _queue;
    public ConsumerFactory(string queue) => _queue = queue;
    public void RegisterConsumer(Type type)
    {
        if (!type.ImplementsGenericInterface(typeof(IConsumer<>)))
            throw new ConfigurationException($"Consumer must implement interface {typeof(IConsumer<>).Name}");

        Consumers.Add(type);
    }
}
