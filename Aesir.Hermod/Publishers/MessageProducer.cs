using Aesir.Hermod.Publishers.Interfaces;
using RabbitMQ.Client;

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
}
