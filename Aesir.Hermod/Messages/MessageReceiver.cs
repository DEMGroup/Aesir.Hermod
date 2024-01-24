using Aesir.Hermod.Bus.Enums;
using Aesir.Hermod.Bus.Interfaces;
using Aesir.Hermod.Consumers.Interfaces;
using Aesir.Hermod.Exceptions;
using Aesir.Hermod.Extensions;
using Aesir.Hermod.Messages.Interfaces;
using Aesir.Hermod.Models;
using Aesir.Hermod.Publishers.Interfaces;
using Aesir.Hermod.Publishers.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Aesir.Hermod.Consumers.Models;

namespace Aesir.Hermod.Messages;

internal class MessageReceiver : IMessageReceiver
{
    private readonly IMessagingBus _messagingBus;
    private readonly IEndpointConsumerFactory _consumerFactory;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMessageProducer _producer;
    private readonly ILogger<object> _logger;

    public MessageReceiver(IEndpointConsumerFactory consumers, IServiceProvider sp)
    {
        _producer = sp.GetRequiredService<IMessageProducer>();
        _messagingBus = sp.GetRequiredService<IMessagingBus>();
        _consumerFactory = consumers;
        _serviceProvider = sp;
        _logger = sp.GetLogger<MessageReceiver>();

        RegisterExchanges(consumers.GetExchanges());
        RegisterQueues(consumers.GetQueues());
        CreateConsumer("amq.rabbitmq.reply-to");
    }

    public void CreateConsumer(string queue)
    {
        var consumer = new EventingBasicConsumer(_messagingBus.GetChannel());
        consumer.Received += ReceiveMessage;
        _messagingBus.GetChannel().BasicConsume(queue, true, consumer);
    }

    private void RegisterExchanges(IEnumerable<(ExchangeDeclaration, string?)> exchanges)
    {
        foreach (var (exchange, routingKey) in exchanges)
        {
            _messagingBus.GetChannel()
                .ExchangeDeclare(exchange.Exchange, exchange.Type, exchange.Durable, exchange.AutoDelete);
            var queueName = _messagingBus.GetChannel().QueueDeclare().QueueName;
            _messagingBus.GetChannel().QueueBind(queueName, exchange.Exchange, routingKey);
            CreateConsumer(queueName);
        }
    }

    private void RegisterQueues(IEnumerable<QueueDeclaration> queues)
    {
        foreach (var queue in queues)
        {
            _messagingBus.GetChannel().QueueDeclare(queue.Queue, queue.Durable, queue.Exclusive, queue.AutoDelete);
            CreateConsumer(queue.Queue);
        }
    }

    private void ReceiveMessage(object? sender, BasicDeliverEventArgs e)
    {
        var res = _messagingBus.GetExpectedResponse(e.BasicProperties.CorrelationId);

        if (!e.RoutingKey.StartsWith("amq.rabbitmq.reply-to")) ProcessMessage(e);
        else if (res != null) ProcessResult(e, res);
    }

    private static void ProcessResult(BasicDeliverEventArgs e, ExpectedResponse response)
    {
        try
        {
            var bodyMsgStr = Encoding.UTF8.GetString(e.Body.ToArray());
            var bodyMsg = JsonSerializer.Deserialize<MessageWrapper>(bodyMsgStr);
            if (bodyMsg == null) return;

            if (bodyMsg.Message == null)
            {
                response.Action(null);
                return;
            }

            var res = JsonSerializer.Deserialize(bodyMsg.Message, response.Type);
            if (res == null) return;

            response.Action(res);
        }
        catch
        {
            // Handle errors
        }
    }

    private readonly List<string> _ignoredQueues = new();
    private readonly List<string> _ignoredTypes = new();

    private void ProcessMessage(BasicDeliverEventArgs e)
    {
        var scope = _serviceProvider.CreateScope();

        try
        {
            var isExchange = string.IsNullOrEmpty(e.Exchange);
            var name = !isExchange ? e.RoutingKey : e.Exchange;
            var routingKey = !isExchange ? null : e.RoutingKey;

            _logger.LogDebug("[Message:{messageId}] Message recieved on queue {queue}", e.BasicProperties.MessageId,
                name);

            if (_ignoredQueues.Contains(name)) return;
            var consumers = _consumerFactory.Get(
                name,
                isExchange ? EndpointType.Exchange : EndpointType.Queue,
                routingKey
            );

            var endpointConsumers = consumers as EndpointConsumer[] ?? consumers.ToArray();
            if (!endpointConsumers.Any())
            {
                _ignoredQueues.Add(name);
                _logger.LogDebug("[Message:{messageId}], No consumer registered for {queue} adding to ignore.",
                    e.BasicProperties.MessageId, name);
                return;
            }

            var bodyMsgStr = Encoding.UTF8.GetString(e.Body.ToArray());
            var bodyMsg = JsonSerializer.Deserialize<MessageWrapper>(bodyMsgStr);

            if (bodyMsg == null)
            {
                _logger.LogDebug("[Message:{messageId}] The deserialized message was null.",
                    e.BasicProperties.MessageId);
                return;
            }

            if (string.IsNullOrEmpty(bodyMsg.Message))
            {
                _logger.LogDebug("[Message:{messageId}] The message had no body content.", e.BasicProperties.MessageId);
                return;
            }

            if (string.IsNullOrEmpty(bodyMsg.Type))
            {
                _logger.LogDebug("[Message:{messageId}] The message didn't contain a deserialization type.",
                    e.BasicProperties.MessageId);
                return;
            }

            if (_ignoredTypes.Contains(bodyMsg.Type)) return;
            var consumerMethod = endpointConsumers.FirstOrDefault()!.Registry.Find(bodyMsg.Type);
            if (consumerMethod == null)
            {
                _logger.LogDebug(
                    "[Message:{messageId}] No consumer found for the type of message {type} adding to ignore list.",
                    e.BasicProperties.MessageId, bodyMsg.Type);
                _ignoredTypes.Add(bodyMsg.Type);
                return;
            }

            var method = consumerMethod.GetMethod(nameof(IConsumer<IMessage>.Consume));
            if (method == null)
                throw new MessageReceiveException("Failed to get to consume method of the registered IConsumer.");

            var parameter = method.GetParameters().FirstOrDefault()?.ParameterType.GenericTypeArguments
                .FirstOrDefault();
            if (parameter == null)
                throw new MessageReceiveException(
                    "Failed to get to get the registered IMessage type for this IConsumer.");

            var msg = JsonSerializer.Deserialize(bodyMsg.Message, parameter);
            if (msg == null)
                throw new MessageReceiveException("Failed to deserialize the IMessage.");

            var ctxType = typeof(MessageContext<>);
            var constructedCtx = ctxType.MakeGenericType(parameter);

            var obj = Activator.CreateInstance(constructedCtx,
                new object[] { msg, e, _serviceProvider.GetRequiredService<IMessageProducer>() });
            var instance = ActivatorUtilities.CreateInstance(scope.ServiceProvider, consumerMethod);
            method.Invoke(instance, new List<object> { obj! }.ToArray());

            var prop = constructedCtx.GetProperty(nameof(MessageContext<IMessage>.HasReplied),
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (prop!.GetValue(obj) is bool val && !val)
            {
                _producer.SendEmpty(e.BasicProperties.CorrelationId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            _logger.LogError(ex.StackTrace);
            throw new MessageReceiveException(ex.Message);
        }
    }
}