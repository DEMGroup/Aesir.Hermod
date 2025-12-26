using Aesir.Hermod.Bus.Interfaces;
using Aesir.Hermod.Exceptions;
using Aesir.Hermod.Extensions;
using Aesir.Hermod.Logging;
using Aesir.Hermod.Messages.Interfaces;
using Aesir.Hermod.Models;
using Aesir.Hermod.Publishers.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Aesir.Hermod.Publishers;

/// <summary>
/// Contains all methods for sending messages
/// </summary>
public class MessageProducer : IInternalMessageProducer
{
    private readonly string _replyQueue;
    private readonly TimeSpan _timeout;
    private readonly ILogger<MessageProducer> _logger;
    private readonly IMessagingBus _messagingBus;
    private readonly List<string> _registeredQueues = new List<string>();
    private readonly List<string> _registeredExchanges = new List<string>();

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
        _logger = sp.GetLogger<MessageProducer>();
    }

    private BasicProperties CreateProperties(bool isReplyTo)
    {
        var props = new BasicProperties
        {
            ReplyTo = !isReplyTo ? null : _replyQueue,
            CorrelationId = Guid.NewGuid().ToString()
        };
        return props;
    }

    /// <inheritdoc/>
    public void SendToExchange<T>(
        T message,
        ExchangeDeclaration exchange,
        string? routingKey = null) where T : IMessage
        => Send(message, exchange.Exchange, routingKey, exchange, null);

    /// <inheritdoc/>
    public void Send<T>(
        T message,
        QueueDeclaration queue
    ) where T : IMessage
        => Send(message, null, queue.Queue, null, queue);

    /// <inheritdoc/>
    public Task<TResult?> SendWithResponseAsync<TResult, T>(
        T message,
        QueueDeclaration declaration)
        where T : IMessage where TResult : IMessageResult<T>
    {
        var correlationId = Send(message, null, declaration.Queue, null, declaration);
        ;

        var tcs = new TaskCompletionSource<TResult?>();
        _messagingBus.RegisterResponseExpected(correlationId, (obj) =>
        {
            switch (obj)
            {
                case null:
                    tcs.SetResult(default);
                    break;
                case TResult response:
                    tcs.SetResult(response);
                    break;
            }
        }, typeof(TResult));

        return tcs.Task.TimeoutAfter(_timeout, () => _messagingBus.RemoveCorrelationCallback(correlationId));
    }

    private string Send<T>(
        T message,
        string? exchange,
        string? routingKey,
        ExchangeDeclaration? exchangeDeclaration,
        QueueDeclaration? queueDeclaration) where T : IMessage
    {
        if (string.IsNullOrEmpty(exchange) && string.IsNullOrEmpty(routingKey))
            throw new MessagePublishException("You must specify a queue or exchange, both cannot be null or empty.");

        if (!string.IsNullOrEmpty(exchange) && !_registeredExchanges.Contains(exchange))
            DeclareExchange(exchangeDeclaration);

        if (string.IsNullOrEmpty(exchange) && !_registeredQueues.Contains(routingKey!))
            DeclareQueue(queueDeclaration);

        var msgWrapper = new MessageWrapper
            { Message = JsonSerializer.Serialize((object)message), Type = message.GetType().Name };

        var msg = JsonSerializer.Serialize(msgWrapper);
        var msgBytes = Encoding.UTF8.GetBytes(msg);

        var props = CreateProperties(exchange is null);

        try
        {
            _messagingBus.GetChannel().BasicPublishAsync(
                exchange: exchange ?? "",
                routingKey: routingKey ?? "",
                mandatory: false,
                basicProperties: props,
                body: msgBytes).GetAwaiter().GetResult();
        }
        catch(Exception ex)
        {
            _registeredExchanges.Clear();
            _registeredQueues.Clear();
            throw;
        }

        return props.CorrelationId;
    }

    private void DeclareExchange(ExchangeDeclaration? exchangeDeclaration)
    {
        if (exchangeDeclaration is null) throw new Exception("Attempted to send message to an undeclared exchange.");
        _messagingBus.GetChannel().ExchangeDeclareAsync(
            exchangeDeclaration.Exchange,
            exchangeDeclaration.Type,
            exchangeDeclaration.Durable,
            exchangeDeclaration.AutoDelete,
            exchangeDeclaration.Arguments).GetAwaiter().GetResult();

        _registeredExchanges.Add(exchangeDeclaration.Exchange);
    }

    private void DeclareQueue(QueueDeclaration? queueDeclaration)
    {
        if (queueDeclaration is null) throw new Exception("Attempted to send message to an undeclared queue.");
        _messagingBus.GetChannel().QueueDeclareAsync(
            queueDeclaration.Queue,
            queueDeclaration.Durable,
            queueDeclaration.Exclusive,
            queueDeclaration.AutoDelete).GetAwaiter().GetResult();
        _registeredQueues.Add(queueDeclaration.Queue);
    }

    /// <inheritdoc/>
    public void Respond<TResult, T>(TResult message, string correlationId, string replyTo)
        where T : IMessage
        where TResult : IMessageResult<T>
    {
        var replyProps = CreateProperties(false);
        replyProps.CorrelationId = correlationId;

        var msgWrapper = new MessageWrapper
            { Message = JsonSerializer.Serialize((object)message), Type = message.GetType().Name };

        var msg = JsonSerializer.Serialize(msgWrapper);
        var msgBytes = Encoding.UTF8.GetBytes(msg);

        _messagingBus.GetChannel().BasicPublishAsync(
            exchange: "",
            routingKey: replyTo,
            mandatory: false,
            basicProperties: replyProps,
            body: msgBytes).GetAwaiter().GetResult();
    }

    /// <inheritdoc/>
    public void SendEmpty(string correlationId)
    {
        var replyProps = CreateProperties(false);
        replyProps.CorrelationId = correlationId;

        var msg = new MessageWrapper();

        var msgStr = JsonSerializer.Serialize(msg);
        var msgBytes = Encoding.UTF8.GetBytes(msgStr);

        _messagingBus.GetChannel().BasicPublishAsync(
            exchange: "",
            routingKey: _replyQueue,
            mandatory: false,
            basicProperties: replyProps,
            body: msgBytes).GetAwaiter().GetResult();
    }
}