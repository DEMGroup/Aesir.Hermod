using Aesir.Hermod.Bus.Buses;
using Aesir.Hermod.Bus.Configuration;
using Aesir.Hermod.Bus.Enums;
using Aesir.Hermod.Bus.Interfaces;
using Aesir.Hermod.Configuration.Interfaces;
using Aesir.Hermod.Consumers;
using Aesir.Hermod.Consumers.Interfaces;
using Aesir.Hermod.Extensions;
using Aesir.Hermod.Messages;
using Aesir.Hermod.Messages.Interfaces;
using Aesir.Hermod.Models;
using Aesir.Hermod.Publishers;
using Aesir.Hermod.Publishers.Configuration;
using Aesir.Hermod.Publishers.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Aesir.Hermod.Configuration;

/// <summary>
/// Used in the configuration pipeline to register consumers and create the <see cref="IMessagingBus"/>.
/// </summary>
public class ConfigurationBuilder : IConfigurationBuilder
{
    private readonly EndpointConsumerFactory _endpointConsumerFactory;
    private readonly IServiceProvider _sp;
    private readonly ILogger<ConfigurationBuilder> _logger;
    private readonly MessageProducer _producer;
    internal BusOptions BusOptions { get; set; } = new();
    internal ProducerOptions ProducerOptions { get; set; } = new();

    /// <summary>
    /// Creates a new instance of the <see cref="ConfigurationBuilder"/> class.
    /// </summary>
    public ConfigurationBuilder(IServiceProvider sp)
    {
        _endpointConsumerFactory = new EndpointConsumerFactory();
        _sp = sp;
        _logger = sp.GetLogger<ConfigurationBuilder>();
        _producer = new MessageProducer(sp, ProducerOptions.ResponseTimeout);
    }

    /// <summary>
    /// Configures the RabbitMQ connection.
    /// </summary>
    /// <param name="configure"></param>
    public void ConfigureHost(Action<BusOptions> configure)
    {
        configure.Invoke(BusOptions);
        _logger.LogDebug(
            "Configured bus options as - (Host: {host}, Port: {port}, VHost: {vhost}, User: {user}, RetryConnection: {retry}, RetryWait: {wait})",
            BusOptions.Host, BusOptions.Port, BusOptions.VHost, BusOptions.User, BusOptions.RetryConnection,
            BusOptions.RetryWaitTime);
    }

    internal IMessagingBus ConfigureBus()
    {
        _logger.LogDebug(
            "Configuring rabbit bus with options - (Host: {host}, Port: {port}, VHost: {vhost}, User: {user}, RetryConnection: {retry}, RetryWait: {wait})",
            BusOptions.Host, BusOptions.Port, BusOptions.VHost, BusOptions.User, BusOptions.RetryConnection,
            BusOptions.RetryWaitTime);
        return new RabbitMqBus(_sp, new ConnectionFactory
        {
            UserName = BusOptions.User,
            Password = BusOptions.Pass,
            HostName = BusOptions.Host,
            Port = BusOptions.Port,
            VirtualHost = BusOptions.VHost,
            AutomaticRecoveryEnabled = BusOptions.RetryConnection,
            NetworkRecoveryInterval = TimeSpan.FromSeconds(BusOptions.RetryWaitTime)
        });
    }

    internal IMessageReceiver ConfigureReceiver(IServiceProvider sp) =>
        new MessageReceiver(_endpointConsumerFactory, sp);

    internal IMessageProducer ConfigureProducer(IServiceProvider sp) =>
        _producer;

    internal IInternalMessageProducer ConfigureInternalProducer(IServiceProvider sp) =>
        _producer;

    /// <summary>
    /// Registers an <see cref="IConsumer{T}"/> to be used for the specified queue.
    /// </summary>
    /// <param name="queue"></param>
    /// <param name="configure"></param>
    public void ConsumeQueue(
        QueueDeclaration queue,
        Action<IConsumerRegistry> configure)
    {
        _logger.LogDebug("Registering a consumer for queue {queue}", queue);
        var consumerFac = new ConsumerRegistry(_sp);
        configure.Invoke(consumerFac);
        _endpointConsumerFactory.AddQueue(queue, consumerFac);
        _logger.LogDebug("Registered a consumer for queue {queue}", queue);
    }

    /// <summary>
    /// Registers an <see cref="IConsumer{T}"/> to be used for the specified queues.
    /// </summary>
    /// <param name="queues"></param>
    /// <param name="configure"></param>
    public void ConsumeQueues(IEnumerable<QueueDeclaration> queues, Action<IConsumerRegistry> configure)
    {
        foreach (var queue in queues)
        {
            _logger.LogDebug("Registering a consumer for queue {queue}", queue);
            var consumerFac = new ConsumerRegistry(_sp);
            configure.Invoke(consumerFac);
            _endpointConsumerFactory.AddQueue(queue, consumerFac);
            _logger.LogDebug("Registered a consumer for queue {queue}", queue);
        }
    }


    /// <summary>
    /// Registers an <see cref="IConsumer{T}"/> to be used for the specified exchange.
    /// </summary>
    /// <param name="exchange"></param>
    /// <param name="configure"></param>
    public void ConsumeExchange(
        ExchangeDeclaration exchange,
        Action<IConsumerRegistry> configure)
        => ConsumeExchange(exchange, null, configure);

    /// <summary>
    /// Registers an <see cref="IConsumer{T}"/> to be used for the specified exchange.
    /// </summary>
    /// <param name="exchange"></param>
    /// <param name="routingKey"></param>
    /// <param name="configure"></param>
    public void ConsumeExchange(
        ExchangeDeclaration exchange,
        string? routingKey,
        Action<IConsumerRegistry> configure)
    {
        _logger.LogDebug("Registering a consumer for exchange {exchange}", exchange);
        var consumerFac = new ConsumerRegistry(_sp);
        configure.Invoke(consumerFac);
        _endpointConsumerFactory.AddExchange(exchange, consumerFac, routingKey);
        _logger.LogDebug("Registered a consumer for exchange {exchange}", exchange);
    }

    /// <summary>
    /// Configures the producer specific parameters
    /// </summary>
    /// <param name="configure"></param>
    public void ConfigureProducer(Action<ProducerOptions> configure)
    {
        configure.Invoke(ProducerOptions);
        _logger.LogDebug("Configured producer with options - (Timeout: {timeout})", ProducerOptions.ResponseTimeout);
    }
}