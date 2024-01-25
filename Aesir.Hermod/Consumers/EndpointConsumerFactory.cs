using Aesir.Hermod.Bus.Enums;
using Aesir.Hermod.Consumers.Interfaces;
using Aesir.Hermod.Consumers.Models;
using Aesir.Hermod.Exceptions;
using Aesir.Hermod.Models;

namespace Aesir.Hermod.Consumers;

internal class EndpointConsumerFactory : IEndpointConsumerFactory
{
    private readonly List<EndpointConsumer<QueueDeclaration>> _queueConsumers = new();
    private readonly List<EndpointConsumer<ExchangeDeclaration>> _exchangeConsumers = new();

    public void AddQueue(
        QueueDeclaration declaration,
        ConsumerRegistry registry
    )
    {
        if (_queueConsumers.Any(x => x.Name == declaration.Queue))
            throw new ConfigurationException(
                $"Consumers for endpoint {declaration.Queue} have already been configured.");
        _queueConsumers.Add(new EndpointConsumer<QueueDeclaration>(
            declaration.Queue,
            EndpointType.Queue,
            registry,
            declaration)
        );
    }

    public void AddExchange(
        ExchangeDeclaration declaration,
        ConsumerRegistry registry,
        string? routingKey = null
    )
    {
        if (_exchangeConsumers.Any(x =>
                x.Name == declaration.Exchange && x.RoutingKey is not null && x.RoutingKey == routingKey))
            throw new ConfigurationException(
                $"Consumers for endpoint {declaration.Exchange} have already been configured.");

        _exchangeConsumers.Add(new EndpointConsumer<ExchangeDeclaration>(
            declaration.Exchange,
            EndpointType.Exchange,
            registry,
            declaration,
            routingKey)
        );
    }

    public EndpointConsumer? Get(string queue, EndpointType type, string? routingKey = null)
        => _queueConsumers
            .Select(EndpointConsumer.FromTypedConsumer)
            .Concat(_queueConsumers.Select(EndpointConsumer.FromTypedConsumer))
            .FirstOrDefault(x =>
                x.Name == queue &&
                x.EndpointType == type &&
                (routingKey is null || x.RoutingKey == routingKey));

    public IEnumerable<QueueDeclaration> GetQueues()
        => _queueConsumers.Select(x => x.Declaration);

    public IEnumerable<(ExchangeDeclaration, string?)> GetExchanges()
        => _exchangeConsumers.Select(x => (x.Declaration, x.RoutingKey));
}