using Aesir.Hermod.Bus.Configuration;
using Aesir.Hermod.Bus.Enums;
using Aesir.Hermod.Bus.Interfaces;
using Aesir.Hermod.Configuration.Interfaces;
using Aesir.Hermod.Consumers;
using Aesir.Hermod.Consumers.Interfaces;
using Aesir.Hermod.Consumers.Models;
using Aesir.Hermod.Exceptions;

namespace Aesir.Hermod.Configuration;

/// <summary>
/// Used in the configuration pipeline to register consumers and create the <see cref="IMessagingBus"/>.
/// </summary>
public class ConfigurationBuilder : IConfigurationBuilder
{
    private readonly List<EndpointConsumer> _consumers = new();
    internal BusOptions? BusOptions { get; set; }

    /// <summary>
    /// Configures the RabbitMQ connection.
    /// </summary>
    /// <param name="configure"></param>
    public void ConfigureHost(Action<BusOptions> configure)
    {
        var busOptions = new BusOptions();
        configure.Invoke(busOptions);
        BusOptions = busOptions;
    }

    internal IMessagingBus? ConfigureBus(IServiceProvider sp)
    {

        return null;
    }

    /// <inheritdoc/>
    public void ConsumeQueue(string queue, Action<IConsumerRegistry> configure)
    {
        var consumerFac = new ConsumerRegistry();
        configure.Invoke(consumerFac);
        if (_consumers.Any(x => x.RoutingKey == queue && x.EndpointType == EndpointType.Queue))
            throw new ConfigurationException($"Consumers for endpoint {queue} have already been configured.");
        _consumers.Add(new EndpointConsumer(queue, EndpointType.Queue, consumerFac));
    }

    /// <inheritdoc/>
    public void ConsumeExchange(string exchange, Action<IConsumerRegistry> configure)
    {
        var consumerFac = new ConsumerRegistry();
        configure.Invoke(consumerFac);
        if (_consumers.Any(x => x.RoutingKey == exchange && x.EndpointType == EndpointType.Queue))
            throw new ConfigurationException($"Consumers for endpoint {exchange} have already been configured.");
        _consumers.Add(new EndpointConsumer(exchange, EndpointType.Queue, consumerFac));
    }
}
