using Aesir.Hermod.Bus.Buses;
using Aesir.Hermod.Bus.Configuration;
using Aesir.Hermod.Bus.Enums;
using Aesir.Hermod.Bus.Interfaces;
using Aesir.Hermod.Configuration.Interfaces;
using Aesir.Hermod.Consumers;
using Aesir.Hermod.Consumers.Interfaces;
using Aesir.Hermod.Messages;
using Aesir.Hermod.Messages.Interfaces;
using Aesir.Hermod.Publishers;
using Aesir.Hermod.Publishers.Configuration;
using Aesir.Hermod.Publishers.Interfaces;
using RabbitMQ.Client;

namespace Aesir.Hermod.Configuration;

/// <summary>
/// Used in the configuration pipeline to register consumers and create the <see cref="IMessagingBus"/>.
/// </summary>
public class ConfigurationBuilder : IConfigurationBuilder
{
    private readonly EndpointConsumerFactory _endpointConsumerFactory;
    internal BusOptions BusOptions { get; set; } = new BusOptions();
    internal ProducerOptions ProducerOptions { get; set; } = new ProducerOptions();

    /// <summary>
    /// Creates a new instance of the <see cref="ConfigurationBuilder"/> class.
    /// </summary>
    public ConfigurationBuilder()
    {
        _endpointConsumerFactory = new EndpointConsumerFactory();
    }

    /// <summary>
    /// Configures the RabbitMQ connection.
    /// </summary>
    /// <param name="configure"></param>
    public void ConfigureHost(Action<BusOptions> configure) => configure.Invoke(BusOptions);

    internal IMessagingBus ConfigureBus() =>
        new RabbitMqBus(BusOptions, new ConnectionFactory
        {
            UserName = BusOptions.User,
            Password = BusOptions.Pass,
            HostName = BusOptions.Host,
            Port = BusOptions.Port,
            VirtualHost = BusOptions.VHost
        });

    internal IMessageReceiver ConfigureReceiver(IServiceProvider sp) => new MessageReceiver(_endpointConsumerFactory, sp);

    internal IMessageProducer ConfigureProducer(IServiceProvider sp) => new MessageProducer(sp, ProducerOptions.ResponseTimeout);

    /// <summary>
    /// Registers an <see cref="IConsumer{T}"/> to be used for the specified exchange.
    /// </summary>
    /// <param name="queue"></param>
    /// <param name="configure"></param>
    public void ConsumeQueue(string queue, Action<IConsumerRegistry> configure)
    {
        var consumerFac = new ConsumerRegistry();
        configure.Invoke(consumerFac);
        _endpointConsumerFactory.Add(queue, EndpointType.Queue, consumerFac);
    }

    /// <summary>
    /// Registers an <see cref="IConsumer{T}"/> to be used for the specified queue.
    /// </summary>
    /// <param name="exchange"></param>
    /// <param name="configure"></param>
    public void ConsumeExchange(string exchange, Action<IConsumerRegistry> configure)
    {
        var consumerFac = new ConsumerRegistry();
        configure.Invoke(consumerFac);
        _endpointConsumerFactory.Add(exchange, EndpointType.Exchange, consumerFac);
    }

    /// <summary>
    /// Configures the producer specific parameters
    /// </summary>
    /// <param name="configure"></param>
    public void ConfigureProducer(Action<ProducerOptions> configure) => configure.Invoke(ProducerOptions);
}
