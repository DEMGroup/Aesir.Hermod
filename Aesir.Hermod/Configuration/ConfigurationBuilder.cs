using Aesir.Hermod.Bus.Buses;
using Aesir.Hermod.Bus.Configuration;
using Aesir.Hermod.Bus.Enums;
using Aesir.Hermod.Bus.Interfaces;
using Aesir.Hermod.Configuration.Interfaces;
using Aesir.Hermod.Consumers;
using Aesir.Hermod.Consumers.Interfaces;

namespace Aesir.Hermod.Configuration;

/// <summary>
/// Used in the configuration pipeline to register consumers and create the <see cref="IMessagingBus"/>.
/// </summary>
public class ConfigurationBuilder : IConfigurationBuilder
{
    private readonly EndpointConsumerFactory _endpointConsumerFactory;
    internal BusOptions BusOptions { get; set; } = new BusOptions();

    public ConfigurationBuilder()
    {
        _endpointConsumerFactory = new EndpointConsumerFactory();
    }

    /// <summary>
    /// Configures the RabbitMQ connection.
    /// </summary>
    /// <param name="configure"></param>
    public void ConfigureHost(Action<BusOptions> configure) => configure.Invoke(BusOptions);

    internal IMessagingBus ConfigureBus(IServiceProvider sp) => new RabbitMqBus(BusOptions, _endpointConsumerFactory, sp);

    /// <inheritdoc/>
    public void ConsumeQueue(string queue, Action<IConsumerRegistry> configure)
    {
        var consumerFac = new ConsumerRegistry();
        configure.Invoke(consumerFac);
        _endpointConsumerFactory.Add(queue, EndpointType.Queue, consumerFac);
    }

    /// <inheritdoc/>
    public void ConsumeExchange(string exchange, Action<IConsumerRegistry> configure)
    {
        var consumerFac = new ConsumerRegistry();
        configure.Invoke(consumerFac);
        _endpointConsumerFactory.Add(exchange, EndpointType.Exchange, consumerFac);
    }
}
