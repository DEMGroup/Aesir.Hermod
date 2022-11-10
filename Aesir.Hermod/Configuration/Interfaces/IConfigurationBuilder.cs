using Aesir.Hermod.Bus.Configuration;
using Aesir.Hermod.Bus.Interfaces;
using Aesir.Hermod.Consumers.Interfaces;

namespace Aesir.Hermod.Configuration.Interfaces;

/// <summary>
/// Used in the configuration pipeline to register consumers and create the <see cref="IMessagingBus"/>.
/// </summary>
public interface IConfigurationBuilder
{
    /// <summary>
    /// Configures the RabbitMQ connection.
    /// </summary>
    /// <param name="configure"></param>
    void ConfigureHost(Action<BusOptions> configure);

    /// <summary>
    /// Registers an <see cref="IConsumer{T}"/> to be used for the specified queue.
    /// </summary>
    /// <param name="queue"></param>
    /// <param name="configure"></param>
    void ConsumeQueue(string queue, Action<IConsumerRegistry> configure);

    /// <summary>
    /// Registers an <see cref="IConsumer{T}"/> to be used for the specified exchange.
    /// </summary>
    /// <param name="queue"></param>
    /// <param name="configure"></param>
    void ConsumeExchange(string queue, Action<IConsumerRegistry> configure);
}
