using System.Collections.ObjectModel;

namespace Aesir.Hermod.Consumers.Interfaces;

/// <summary>
/// Contains relevant methods for registering message consumers.
/// This is created as part of the DI pipeline.
/// </summary>
public interface IConsumerRegistry
{
    /// <summary>
    /// Registers a consumer with the messaging bus.
    /// </summary>
    /// <param name="type"></param>
    void RegisterConsumer(Type type);
}
