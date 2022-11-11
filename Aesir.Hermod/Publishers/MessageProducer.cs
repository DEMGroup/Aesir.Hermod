using Aesir.Hermod.Bus.Interfaces;
using Aesir.Hermod.Exceptions;
using Aesir.Hermod.Extensions;
using Aesir.Hermod.Messages.Interfaces;
using Aesir.Hermod.Models;
using Aesir.Hermod.Publishers.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace Aesir.Hermod.Publishers;

/// <summary>
/// Contains all methods for sending messages
/// </summary>
public class MessageProducer : IMessageProducer
{
    private readonly string _replyQueue;
    private readonly TimeSpan _timeout;
    private readonly IMessagingBus _messagingBus;

    /// <summary>
    /// Creates a new instance of the <see cref="MessageProducer"/> class.
    /// </summary>
    /// <param name="sp"></param>
    /// <param name="timeout"></param>
    public MessageProducer(IServiceProvider sp, TimeSpan timeout)
    {
        _messagingBus = sp.GetRequiredService<IMessagingBus>();
        _replyQueue = "amq.rabbitmq.reply-to";
        _timeout = timeout;
    }

    internal IBasicProperties CreateProperties()
    {
        var props = _messagingBus.GetChannel().CreateBasicProperties();
        props.ReplyTo = _replyQueue;
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        return props;
    }

    /// <inheritdoc/>
    public void SendToExchange<T>(T message, string exchange) where T : IMessage
        => Send(message, exchange, "");

    /// <inheritdoc/>
    public void Send<T>(T message, string queue) where T : IMessage
        => Send(message, null, queue);

    /// <inheritdoc/>
    public Task<TResult?> SendWithResponseAsync<TResult, T>(T message, string? queue) where T : IMessage where TResult : IMessageResult<T>
    {
        var correlationId = Send(message, null, queue);

        var tcs = new TaskCompletionSource<TResult?>();
        _messagingBus.RegisterResponseExpected<TResult>(correlationId, (obj) =>
        {
            if (obj == null) tcs.SetResult(default);
            if (obj is TResult response) tcs.SetResult(response);
        }, typeof(TResult));

        return tcs.Task.TimeoutAfter(_timeout, () => _messagingBus.RemoveCorrelationCallback(correlationId));
    }

    private string Send<T>(T message, string? exchange, string? routingKey) where T : IMessage
    {
        if (string.IsNullOrEmpty(exchange) && string.IsNullOrEmpty(routingKey))
            throw new MessagePublishException("You must specify a queue or exchange, both cannot be null or empty.");

        var msgWrapper = new MessageWrapper { Message = JsonSerializer.Serialize((object)message), Type = message.GetType().Name };

        var msg = JsonSerializer.Serialize(msgWrapper);
        var msgBytes = Encoding.UTF8.GetBytes(msg);

        var props = CreateProperties();

        _messagingBus.GetChannel().BasicPublish(
            exchange: exchange ?? "",
            routingKey: routingKey ?? "",
            basicProperties: props,
            body: msgBytes);

        return props.CorrelationId;
    }

    /// <inheritdoc/>
    public void Respond<TResult, T>(TResult message, string correlationId, string replyTo)
        where T : IMessage
        where TResult : IMessageResult<T>
    {
        var replyProps = CreateProperties();
        replyProps.CorrelationId = correlationId;

        var msgWrapper = new MessageWrapper { Message = JsonSerializer.Serialize((object)message), Type = message.GetType().Name };

        var msg = JsonSerializer.Serialize(msgWrapper);
        var msgBytes = Encoding.UTF8.GetBytes(msg);

        _messagingBus.GetChannel().BasicPublish(
            exchange: "",
            routingKey: replyTo,
            basicProperties: replyProps,
            body: msgBytes);
    }

    /// <inheritdoc/>
    public void SendEmpty(string correlationId)
    {
        var replyProps = CreateProperties();
        replyProps.CorrelationId = correlationId;

        var msg = new MessageWrapper();

        var msgStr = JsonSerializer.Serialize(msg);
        var msgBytes = Encoding.UTF8.GetBytes(msgStr);

        _messagingBus.GetChannel().BasicPublish(
            exchange: "",
            routingKey: _replyQueue,
            basicProperties: replyProps,
            body: msgBytes);
    }
}
