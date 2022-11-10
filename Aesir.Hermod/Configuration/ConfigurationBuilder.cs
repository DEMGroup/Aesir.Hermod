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
    public void ConsumeQueue(string queue, Action<IConsumerFactory> configure)
    {
        var consumerFac = new ConsumerFactory();
        configure.Invoke(consumerFac);
        RegisterConsumers(consumerFac, queue, EndpointType.Queue);
    }

    /// <inheritdoc/>
    public void ConsumeExchange(string exchange, Action<IConsumerFactory> configure)
    {
        var consumerFac = new ConsumerFactory();
        configure.Invoke(consumerFac);
        RegisterConsumers(consumerFac, exchange, EndpointType.Exchange);
    }

    private void RegisterConsumers(ConsumerFactory factory, string routingKey, EndpointType type)
    {
        if (!factory.Consumers.Any()) return;
        var endpoint = _consumers.Where(x => x.RoutingKey == routingKey && x.EndpointType == type).FirstOrDefault();
        if (endpoint == null)
        {
            endpoint = new EndpointConsumer(routingKey, type, new ConsumerRegistry());
            _consumers.Add(endpoint);
        }
        foreach (var consumer in factory.Consumers)
        {
            var added = endpoint.Registry.TryAdd(consumer);
            if (!added)
                throw new ConfigurationException($"Failed to add consumer of type {consumer.Name}.");
        }
    }
}
