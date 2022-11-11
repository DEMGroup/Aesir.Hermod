using Aesir.Hermod.Exceptions;
using Aesir.Hermod.Messages.Interfaces;
using Aesir.Hermod.Models;
using Aesir.Hermod.Publishers.Interfaces;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Aesir.Hermod.Publishers;

internal class MessageProducer : IMessageProducer
{
    private readonly IModel _model;
    private readonly string _replyQueue;
    public MessageProducer(IModel model, string replyQueue)
    {
        _model = model;
        _replyQueue = replyQueue;
    }

    internal IBasicProperties CreateProperties()
    {
        var props = _model.CreateBasicProperties();
        props.ReplyTo = _replyQueue;
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        return props;
    }

    public void Publish<T>(T message, string? exchange, string? routingKey) where T : IMessage
    {
        if (string.IsNullOrEmpty(exchange) && string.IsNullOrEmpty(routingKey))
            throw new MessagePublishException("You must specify a queue or exchange, both cannot be null or empty.");

        var msgWrapper = new MessageWrapper { Message = JsonSerializer.Serialize((object)message), Type = message.GetType().Name };

        var msg = JsonSerializer.Serialize(msgWrapper);
        var msgBytes = Encoding.UTF8.GetBytes(msg);

        _model.BasicPublish(
            exchange: exchange ?? "",
            routingKey: routingKey ?? "",
            basicProperties: CreateProperties(),
            body: msgBytes);
    }
}
