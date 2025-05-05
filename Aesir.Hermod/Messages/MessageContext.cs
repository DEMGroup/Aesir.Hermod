using Aesir.Hermod.Exceptions;
using Aesir.Hermod.Messages.Interfaces;
using Aesir.Hermod.Publishers.Interfaces;
using RabbitMQ.Client.Events;

namespace Aesir.Hermod.Messages;

internal class MessageContext<T> : IMessageContext<T> where T : IMessage
{
    public T? Message { get; set; }
    internal string CorrelationId { get; set; }
    internal string ReplyTo { get; set; }
    internal bool HasReplied { get; private set; }
    private readonly IInternalMessageProducer _producer;

    public MessageContext(
        T message,
        BasicDeliverEventArgs ea,
        IInternalMessageProducer producer
    )
    {
        Message = message;
        CorrelationId = ea.BasicProperties.CorrelationId;
        ReplyTo = ea.BasicProperties.ReplyTo;
        _producer = producer;
    }

    public void Respond<TResult>(TResult message) where TResult : IMessageResult<T>
    {
        if (HasReplied)
            throw new MessagePublishException(
                $"You cannot respond to the same {nameof(MessageContext<IMessage>)} more than once.");

        _producer.Respond<TResult, T>(message, CorrelationId, ReplyTo);
        HasReplied = true;
    }
}