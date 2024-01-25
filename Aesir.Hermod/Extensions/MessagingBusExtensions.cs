using Aesir.Hermod.Bus.Interfaces;
using Aesir.Hermod.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Aesir.Hermod.Extensions;

internal static class MessagingBusExtensions
{
    internal static void RegisterExchanges(
        this IMessagingBus messageBus,
        IEnumerable<(ExchangeDeclaration, string?)> exchanges,
        Action<string?, BasicDeliverEventArgs> action
    )
    {
        foreach (var (exchange, routingKey) in exchanges)
        {
            messageBus.RegisterExchange(exchange, routingKey, action);
        }
    }

    private static void RegisterExchange(
        this IMessagingBus messageBus,
        ExchangeDeclaration exchange,
        string? routingKey,
        Action<string?, BasicDeliverEventArgs> action
    )
    {
        messageBus.GetChannel()
            .ExchangeDeclare(exchange.Exchange, exchange.Type, exchange.Durable, exchange.AutoDelete);
        var queueName = messageBus.GetChannel().QueueDeclare().QueueName;
        messageBus.GetChannel().QueueBind(queueName, exchange.Exchange, routingKey);
        messageBus.RegisterConsumer(queueName, action, routingKey);
    }

    public static void RegisterQueues(
        this IMessagingBus messagingBus,
        IEnumerable<QueueDeclaration> queues,
        Action<string?, BasicDeliverEventArgs> action)
    {
        foreach (var queue in queues)
        {
            messagingBus.RegisterQueue(queue, action);
        }
    }

    private static void RegisterQueue(
        this IMessagingBus messagingBus,
        QueueDeclaration queue,
        Action<string?, BasicDeliverEventArgs> action
    )
    {
        messagingBus.GetChannel().QueueDeclare(queue.Queue, queue.Durable, queue.Exclusive, queue.AutoDelete);
        messagingBus.RegisterConsumer(queue.Queue, action);
    }

    internal static void RegisterConsumer(
        this IMessagingBus messageBus,
        string queue,
        Action<string?, BasicDeliverEventArgs> action,
        string? routingKey = null
    )
    {
        var consumer = new EventingBasicConsumer(messageBus.GetChannel());
        consumer.Received += (_, e) => action(routingKey, e);
        messageBus.GetChannel().BasicConsume(queue, true, consumer);
    }
}