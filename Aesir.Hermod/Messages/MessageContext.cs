﻿using Aesir.Hermod.Messages.Interfaces;
using Aesir.Hermod.Publishers.Interfaces;
using RabbitMQ.Client.Events;

namespace Aesir.Hermod.Messages;

internal class MessageContext<T> : IMessageContext<T> where T : IMessage
{
    public T? Message { get; set; }
    internal string CorrelationId { get; set; }
    internal string ReplyTo { get; set; }
    private readonly IMessageProducer _producer;
    public MessageContext(T message, BasicDeliverEventArgs ea, IMessageProducer producer)
    {
        Message = message;
        CorrelationId = ea.BasicProperties.CorrelationId;
        ReplyTo = ea.BasicProperties.ReplyTo;
        _producer = producer;
    }

    public void Respond<TResult>(TResult message) where TResult : IMessageResult<T>
        => _producer.Respond<TResult, T>(message, CorrelationId, ReplyTo);
}
