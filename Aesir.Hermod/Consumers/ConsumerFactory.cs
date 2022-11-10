using Aesir.Hermod.Consumers.Interfaces;
using Aesir.Hermod.Exceptions;
using Aesir.Hermod.Extensions;

namespace Aesir.Hermod.Consumers;

/// <summary>
/// Used during the configuration pipeline to register <see cref="IConsumer{T}"/>'s.
/// </summary>
public class ConsumerFactory : IConsumerFactory
{
    internal List<Type> Consumers { get; } = new();

    /// <summary>
    /// Inserts the provided type into a list of Consumers, no duplicate checking is done at this point.
    /// </summary>
    /// <param name="type"></param>
    /// <exception cref="ConfigurationException"></exception>
    public void RegisterConsumer(Type type)
    {
        if (!type.ImplementsGenericInterface(typeof(IConsumer<>)))
            throw new ConfigurationException($"Consumer must implement interface {typeof(IConsumer<>).Name}");

        Consumers.Add(type);
    }
}
