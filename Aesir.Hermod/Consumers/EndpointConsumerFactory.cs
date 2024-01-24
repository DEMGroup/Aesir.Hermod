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

    public IEnumerable<EndpointConsumer> Get(string queue, EndpointType type, string? routingKey = null)
        => _queueConsumers
            .Select(EndpointConsumer.FromTypedConsumer)
            .Concat(_queueConsumers.Select(EndpointConsumer.FromTypedConsumer))
            .Where(x =>
                x.Name == queue &&
                x.EndpointType == type &&
                (routingKey is null || MatchRoutingKeys(x.RoutingKey, routingKey)));

    private static bool MatchRoutingKeys(string? key1, string key2)
        => key1 is not null && MatchParts(key1.Split('.'), key2.Split('.'), 0, 0);

    private static bool MatchParts(
        IReadOnlyList<string> parts1, 
        IReadOnlyList<string> parts2, 
        int index1, 
        int index2)
    {
        while (true)
        {
            if (index1 == parts1.Count && index2 == parts2.Count)
            {
                return true;
            }

            if (index1 < parts1.Count && index2 < parts2.Count)
            {
                if (parts1[index1] == "#" || parts2[index2] == "#")
                {
                    return MatchParts(parts1, parts2, parts1.Count, parts2.Count) || MatchParts(parts1, parts2, index1 + 1, index2) || MatchParts(parts1, parts2, index1, index2 + 1);
                }

                if (parts1[index1] == "*" || parts2[index2] == "*" || parts1[index1] == parts2[index2])
                {
                    index1 += 1;
                    index2 += 1;
                    continue;
                }
            }

            if (index1 < parts1.Count && parts1[index1] == "#")
            {
                index1 += 1;
                continue;
            }

            if (index2 >= parts2.Count || parts2[index2] != "#") return false;
            index2 += 1;
        }
    }


    public IEnumerable<QueueDeclaration> GetQueues()
        => _queueConsumers.Select(x => x.Declaration);

    public IEnumerable<(ExchangeDeclaration, string?)> GetExchanges()
        => _exchangeConsumers.Select(x => (x.Declaration, x.RoutingKey));
}