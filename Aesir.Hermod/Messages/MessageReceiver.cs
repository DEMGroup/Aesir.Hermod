using Aesir.Hermod.Consumers.Interfaces;
using Aesir.Hermod.Messages.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

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
        throw new NotImplementedException();
    }
}
