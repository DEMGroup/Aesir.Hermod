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
        if (_queueConsumers.Any(x => x.RoutingKey == declaration.Queue))
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
        ConsumerRegistry registry
    )
    {
        if (_exchangeConsumers.Any(x => x.RoutingKey == declaration.Exchange))
            throw new ConfigurationException(
                $"Consumers for endpoint {declaration.Exchange} have already been configured.");
        _exchangeConsumers.Add(new EndpointConsumer<ExchangeDeclaration>(
            declaration.Exchange,
            EndpointType.Exchange,
            registry, declaration));
    }

    public EndpointConsumer? Get(string queue, EndpointType type)
        => _queueConsumers
            .Select(EndpointConsumer.FromTypedConsumer)
            .Concat(_queueConsumers.Select(EndpointConsumer.FromTypedConsumer))
            .FirstOrDefault(x => x.RoutingKey == queue && x.EndpointType == type);

    public IEnumerable<QueueDeclaration> GetQueues()
        => _queueConsumers.Select(x => x.Declaration);

    public IEnumerable<ExchangeDeclaration> GetExchanges()
        => _exchangeConsumers.Select(x => x.Declaration);
}