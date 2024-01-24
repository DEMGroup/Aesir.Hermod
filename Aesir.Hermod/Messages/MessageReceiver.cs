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

        _messagingBus.RegisterExchanges(consumers.GetExchanges(), ReceiveMessage);
        _messagingBus.RegisterQueues(consumers.GetQueues(), ReceiveMessage);
        _messagingBus.RegisterConsumer("amq.rabbitmq.reply-to", ReceiveMessage);
    }

    private void ReceiveMessage(string? routingKey, BasicDeliverEventArgs e)
    {
        var res = _messagingBus.GetExpectedResponse(e.BasicProperties.CorrelationId);
        if (!e.RoutingKey.StartsWith("amq.rabbitmq.reply-to")) ProcessMessage(e, routingKey);
        else if (res != null) MessageProcessing.ProcessResponse(e, res);
    }

    private readonly List<string> _ignoredQueues = new();
    private readonly List<string> _ignoredTypes = new();

    private void ProcessMessage(
        BasicDeliverEventArgs e,
        string? routingKey)
    {
        var scope = _serviceProvider.CreateScope();

        try
        {
            var isExchange = string.IsNullOrEmpty(e.Exchange);
            var name = !isExchange ? e.RoutingKey : e.Exchange;

            _logger.LogDebug("[Message:{messageId}] Message recieved on queue {queue}", e.BasicProperties.MessageId,
                name);

            if (_ignoredQueues.Contains(name)) return;
            var consumer = _consumerFactory.Get(
                name,
                isExchange ? EndpointType.Exchange : EndpointType.Queue,
                routingKey
            );

            if (consumer is null)
            {
                _ignoredQueues.Add(name);
                _logger.LogDebug("[Message:{messageId}], No consumer registered for {queue} adding to ignore.",
                    e.BasicProperties.MessageId, name);
                return;
            }

            var bodyMsg = MessageProcessing.ParseMessage(e.Body);
            var res = UnwrapMessage(bodyMsg, e.BasicProperties.CorrelationId);
            if (res is null) return;

            var consumerMethod = consumer.Registry.Find(res.Value.Item1);
            if (consumerMethod == null)
            {
                _logger.LogDebug(
                    "[Message:{messageId}] No consumer found for the type of message {type} adding to ignore list.",
                    e.BasicProperties.MessageId, res.Value.Item1);
                _ignoredTypes.Add(res.Value.Item1);
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

            var msg = JsonSerializer.Deserialize(res.Value.Item2, parameter);
            if (msg == null)
                throw new MessageReceiveException("Failed to deserialize the IMessage.");

            var ctxType = typeof(MessageContext<>);
            var constructedCtx = ctxType.MakeGenericType(parameter);

            var obj = Activator.CreateInstance(
                constructedCtx,
                msg,
                e,
                _serviceProvider.GetRequiredService<IMessageProducer>()
            );
            var instance = ActivatorUtilities.CreateInstance(scope.ServiceProvider, consumerMethod);
            method.Invoke(instance, new List<object> { obj! }.ToArray());

            var prop = constructedCtx.GetProperty(nameof(MessageContext<IMessage>.HasReplied),
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (prop!.GetValue(obj) is false)
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

    private (string, string)? UnwrapMessage(
        MessageWrapper? message,
        string messageId)
    {
        if (message == null)
        {
            _logger.LogDebug("[Message:{messageId}] The deserialized message was null.",
                messageId);
            return null;
        }

        if (string.IsNullOrEmpty(message.Message))
        {
            _logger.LogDebug("[Message:{messageId}] The message had no body content.", messageId);
            return null;
        }

        if (!string.IsNullOrEmpty(message.Type))
            return _ignoredTypes.Contains(message.Type) ? null : (message.Type, message.Message);

        _logger.LogDebug("[Message:{messageId}] The message didn't contain a deserialization type.",
            messageId);
        return null;
    }
}