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
        RegisterEndpoints(consumers.GetEndpoints());
        _logger = sp.GetLogger<MessageReceiver>();
    }

    public void CreateConsumer(string queue)
    {
        var consumer = new EventingBasicConsumer(_messagingBus.GetChannel());
        consumer.Received += ReceiveMessage;
        _messagingBus.GetChannel().BasicConsume(queue, true, consumer);
    }

    private void RegisterEndpoints(IEnumerable<(string, EndpointType)> endpoints)
    {
        foreach (var (route, type) in endpoints)
        {
            if (type == EndpointType.Queue)
            {
                _messagingBus.GetChannel().QueueDeclare(route, true, false, false);
                CreateConsumer(route);
            }
            else
            {
                _messagingBus.GetChannel().ExchangeDeclare(route, ExchangeType.Fanout, true);
                var queueName = _messagingBus.GetChannel().QueueDeclare().QueueName;
                _messagingBus.GetChannel().QueueBind(queueName, route, "");
                CreateConsumer(queueName);
            }
        }

        CreateConsumer("amq.rabbitmq.reply-to");
    }

    private void ReceiveMessage(object? sender, BasicDeliverEventArgs e)
    {
        var res = _messagingBus.GetExpectedResponse(e.BasicProperties.CorrelationId);

        if (!e.RoutingKey.StartsWith("amq.rabbitmq.reply-to")) ProcessMessage(e);
        else if (res != null) ProcessResult(e, res);
        else return;
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

    private readonly List<string> _ignoredQueues = new List<string>();
    private readonly List<string> _ignoredTypes = new List<string>();
    private void ProcessMessage(BasicDeliverEventArgs e)
    {
        try
        {
            var queue = string.IsNullOrEmpty(e.Exchange) ? e.RoutingKey : e.Exchange;
            _logger.LogDebug("[Message:{messageId}] Message recieved on queue {queue}", e.BasicProperties.MessageId, queue);

            if (_ignoredQueues.Contains(queue)) return;
            var consumer = _consumerFactory.Get(queue, queue == e.Exchange ? EndpointType.Exchange : EndpointType.Queue);
            if (consumer == null)
            {
                _ignoredQueues.Add(queue);
                _logger.LogDebug("[Message:{messageId}], No consumer registered for {queue} adding to ignore.", e.BasicProperties.MessageId, queue);
                return;
            }

            var bodyMsgStr = Encoding.UTF8.GetString(e.Body.ToArray());
            var bodyMsg = JsonSerializer.Deserialize<MessageWrapper>(bodyMsgStr);

            if (bodyMsg == null)
            {
                _logger.LogDebug("[Message:{messageId}] The deserialized message was null.", e.BasicProperties.MessageId);
                return;
            }
            if (string.IsNullOrEmpty(bodyMsg.Message))
            {
                _logger.LogDebug("[Message:{messageId}] The message had no body content.", e.BasicProperties.MessageId);
                return;
            }
            if (string.IsNullOrEmpty(bodyMsg.Type))
            {
                _logger.LogDebug("[Message:{messageId}] The message didn't contain a deserialization type.", e.BasicProperties.MessageId);
                return;
            }

            if (_ignoredTypes.Contains(bodyMsg.Type)) return;
            var consumerMethod = consumer.Registry.Find(bodyMsg.Type);
            if (consumerMethod == null)
            {
                _logger.LogDebug("[Message:{messageId}] No consumer found for the type of message {type} adding to ignore list.", e.BasicProperties.MessageId, bodyMsg.Type);
                _ignoredTypes.Add(bodyMsg.Type);
                return;
            }

            var method = consumerMethod.GetMethod(nameof(IConsumer<IMessage>.Consume));
            if (method == null) 
                throw new MessageReceiveException("Failed to get to consume method of the registered IConsumer.");

            var parameter = method.GetParameters().FirstOrDefault()?.ParameterType.GenericTypeArguments.FirstOrDefault();
            if (parameter == null) 
                throw new MessageReceiveException("Failed to get to get the registered IMessage type for this IConsumer.");

            var msg = JsonSerializer.Deserialize(bodyMsg.Message, parameter);
            if (msg == null) 
                throw new MessageReceiveException("Failed to deserialize the IMessage.");

            var ctxType = typeof(MessageContext<>);
            var constructedCtx = ctxType.MakeGenericType(parameter);

            var obj = Activator.CreateInstance(constructedCtx, new object[] { msg, e, _serviceProvider.GetRequiredService<IMessageProducer>() });
            var instance = ActivatorUtilities.CreateInstance(_serviceProvider, consumerMethod);
            method.Invoke(instance, new List<object> { obj! }.ToArray());

            var prop = constructedCtx.GetProperty(nameof(MessageContext<IMessage>.HasReplied), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (prop!.GetValue(obj) is bool val && !val)
            {
                _producer.SendEmpty(e.BasicProperties.CorrelationId);
            }
        }
        catch(Exception ex)
        {
            _logger.LogError(ex.Message);
            _logger.LogError(ex.StackTrace);
            throw new MessageReceiveException(ex.Message);
        }
    }
}
