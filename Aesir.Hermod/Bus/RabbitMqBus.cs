using Aesir.Hermod.Bus.Configuration;
using Aesir.Hermod.Bus.Enums;
using Aesir.Hermod.Bus.Interfaces;
using Aesir.Hermod.Consumers.Interfaces;
using Aesir.Hermod.Messages;
using Aesir.Hermod.Messages.Interfaces;
using Aesir.Hermod.Publishers;
using Aesir.Hermod.Publishers.Interfaces;
using RabbitMQ.Client;

namespace Aesir.Hermod.Bus.Buses;

internal class RabbitMqBus : IMessagingBus
{
    private readonly IModel _model;
    private readonly IMessageReceiver _messageReceiver;
    private readonly IMessageProducer _messageProducer;

    internal RabbitMqBus(BusOptions opts, IEndpointConsumerFactory endpointConsumerFac, IServiceProvider sp)
    {
        var connFactory = new ConnectionFactory
        {
            UserName = opts.User,
            Password = opts.Pass,
            HostName = opts.Host,
            Port = opts.Port,
            VirtualHost = opts.VHost
        };
        _model = connFactory.CreateConnection().CreateModel();

        var replyQueue = _model.QueueDeclare().QueueName;
        _messageReceiver = new MessageReceiver(_model, endpointConsumerFac, sp);
        _messageProducer = new MessageProducer(_model, replyQueue);

        RegisterEndpoints(replyQueue, endpointConsumerFac.GetEndpoints());
    }

    private void RegisterEndpoints(string replyQueue, IEnumerable<(string, EndpointType)> endpoints)
    {
        foreach (var (route, type) in endpoints)
        {
            if (type == EndpointType.Queue)
            {
                _model.QueueDeclare(route, true, true, true);
                _messageReceiver.CreateConsumer(route);
            }
            else
            {
                _model.ExchangeDeclare(route, ExchangeType.Fanout, true, true);
                var queueName = _model.QueueDeclare().QueueName;
                _model.QueueBind(queueName, route, "");
                _messageReceiver.CreateConsumer(route);
            }
        }

        _model.QueueDeclare(replyQueue, true, true, true);
    }
}
