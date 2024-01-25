using Aesir.Hermod.Bus.Configuration;
using Aesir.Hermod.Bus.Interfaces;
using Aesir.Hermod.Consumers.Interfaces;
using Aesir.Hermod.Models;
using Aesir.Hermod.Publishers.Configuration;

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
    /// Configures the producer specific parameters
    /// </summary>
    /// <param name="configure"></param>
    void ConfigureProducer(Action<ProducerOptions> configure);

    /// <summary>
    /// Registers an <see cref="IConsumer{T}"/> to be used for the specified queue.
    /// </summary>
    /// <param name="queue"></param>
    /// <param name="configure"></param>
    void ConsumeQueue(
        QueueDeclaration queue, 
        Action<IConsumerRegistry> configure);

    /// <summary>
    /// Registers an <see cref="IConsumer{T}"/> to be used for the specified queues.
    /// </summary>
    /// <param name="queues"></param>
    /// <param name="configure"></param>
    void ConsumeQueues(
        IEnumerable<QueueDeclaration> queues, 
        Action<IConsumerRegistry> configure);

    /// <summary>
    /// Registers an <see cref="IConsumer{T}"/> to be used for the specified exchange.
    /// </summary>
    /// <param name="exchange"></param>
    /// <param name="configure"></param>
    void ConsumeExchange(
        ExchangeDeclaration exchange,
        Action<IConsumerRegistry> configure);

    /// <summary>
    /// Registers an <see cref="IConsumer{T}"/> to be used for the specified exchange.
    /// </summary>
    /// <param name="exchange"></param>
    /// <param name="routingKey"></param>
    /// <param name="configure"></param>
    void ConsumeExchange(
        ExchangeDeclaration exchange,
        string? routingKey,
        Action<IConsumerRegistry> configure);
}
