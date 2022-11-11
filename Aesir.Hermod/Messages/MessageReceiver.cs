using Aesir.Hermod.Bus.Enums;
using Aesir.Hermod.Bus.Interfaces;
using Aesir.Hermod.Consumers.Interfaces;
using Aesir.Hermod.Messages.Interfaces;
using Aesir.Hermod.Models;
using Aesir.Hermod.Publishers.Interfaces;
using Aesir.Hermod.Publishers.Models;
using Microsoft.Extensions.DependencyInjection;
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
    public MessageReceiver(IEndpointConsumerFactory consumers, IServiceProvider sp)
    {
        _producer = sp.GetRequiredService<IMessageProducer>();
        _messagingBus = sp.GetRequiredService<IMessagingBus>();
        _consumerFactory = consumers;
        _serviceProvider = sp;
        RegisterEndpoints(consumers.GetEndpoints());
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
                _messagingBus.GetChannel().QueueDeclare(route, true, false, true);
                CreateConsumer(route);
            }
            else
            {
                _messagingBus.GetChannel().ExchangeDeclare(route, ExchangeType.Fanout, true, true);
                var queueName = _messagingBus.GetChannel().QueueDeclare().QueueName;
                _messagingBus.GetChannel().QueueBind(queueName, route, "");
                CreateConsumer(route);
            }
        }

        CreateConsumer("amq.rabbitmq.reply-to");
    }

    private void ReceiveMessage(object? sender, BasicDeliverEventArgs e)
    {
        var res = _messagingBus.GetExpectedResponse(e.BasicProperties.CorrelationId);

        if (res != null) ProcessResult(e, res);
        else if (e.RoutingKey != "amq.rabbitmq.reply-to") ProcessMessage(e);
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

    private void ProcessMessage(BasicDeliverEventArgs e)
    {
        try
        {
            var queue = string.IsNullOrEmpty(e.Exchange) ? e.RoutingKey : e.Exchange;
            var consumer = _consumerFactory.Get(queue, queue == e.Exchange ? EndpointType.Exchange : EndpointType.Queue);
            if (consumer == null) return;

            var bodyMsgStr = Encoding.UTF8.GetString(e.Body.ToArray());
            var bodyMsg = JsonSerializer.Deserialize<MessageWrapper>(bodyMsgStr);
            if (bodyMsg == null || string.IsNullOrEmpty(bodyMsg.Type) || string.IsNullOrEmpty(bodyMsg.Message)) return;

            var consumerMethod = consumer.Registry.Find(bodyMsg.Type);
            if (consumerMethod == null) return;

            var method = consumerMethod.GetMethod(nameof(IConsumer<IMessage>.Consume));
            if (method == null) return;

            var parameter = method.GetParameters().FirstOrDefault()?.ParameterType.GenericTypeArguments.FirstOrDefault();
            if (parameter == null) return;

            var msg = JsonSerializer.Deserialize(bodyMsg.Message, parameter);
            if (msg == null) return;

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
        catch
        {
            // Figure out what should happen on error
        }
    }
}
