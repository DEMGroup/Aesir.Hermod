using Aesir.Hermod.Bus.Enums;
using Aesir.Hermod.Consumers.Interfaces;
using Aesir.Hermod.Messages.Interfaces;
using Aesir.Hermod.Models;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Aesir.Hermod.Messages;

internal class MessageReceiver : IMessageReceiver
{
    private readonly IModel _model;
    private readonly IEndpointConsumerFactory _consumerFactory;
    private readonly IServiceProvider _serviceProvider;
    public MessageReceiver(IModel model, IEndpointConsumerFactory consumers, IServiceProvider sp)
    {
        _model = model;
        _consumerFactory = consumers;
        _serviceProvider = sp;
    }

    public void CreateConsumer(string queue)
    {
        var consumer = new EventingBasicConsumer(_model);
        consumer.Received += ReceiveMessage;
        _model.BasicConsume(queue, false, consumer);
    }

    private void ReceiveMessage(object? sender, BasicDeliverEventArgs e)
    {
        try
        {
            var queue = string.IsNullOrEmpty(e.Exchange) ? e.RoutingKey : e.Exchange;
            var consumer = _consumerFactory.Get(queue, queue == e.Exchange ? EndpointType.Exchange : EndpointType.Queue);
            if (consumer == null) return;

            var bodyMsgStr = Encoding.UTF8.GetString(e.Body.ToArray());
            var bodyMsg = JsonSerializer.Deserialize<MessageWrapper>(bodyMsgStr);
            if(bodyMsg == null) return;

            var consumerMethod = consumer.Registry.Find(bodyMsg.Type);
            if(consumerMethod == null) return;

            var method = consumerMethod.GetMethod(nameof(IConsumer<IMessage>.Consume));
            if (method == null) return;

            var parameter = method.GetParameters().FirstOrDefault()?.ParameterType.GenericTypeArguments.FirstOrDefault();
            if (parameter == null) return;

            var msg = JsonSerializer.Deserialize(bodyMsg.Message, parameter);
            if (msg == null) return;

            var ctxType = typeof(MessageContext<>);
            var constructedCtx = ctxType.MakeGenericType(parameter);

            var obj = Activator.CreateInstance(constructedCtx, new object[] { msg, e });
            var instance = ActivatorUtilities.CreateInstance(_serviceProvider, consumerMethod);
            var response = method.Invoke(instance, new List<object> { obj! }.ToArray());
        }
        catch
        {
            // Figure out what should happen on error
        }
    }
}
