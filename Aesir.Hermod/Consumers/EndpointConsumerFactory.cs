using Aesir.Hermod.Bus.Enums;
using Aesir.Hermod.Consumers.Interfaces;
using Aesir.Hermod.Consumers.Models;
using Aesir.Hermod.Exceptions;

namespace Aesir.Hermod.Consumers;

internal class EndpointConsumerFactory : IEndpointConsumerFactory
{
    private readonly List<EndpointConsumer> _consumers = new();
    public void Add(string queue, EndpointType type, ConsumerRegistry registry)
    {
        if (_consumers.Any(x => x.RoutingKey == queue && x.EndpointType == EndpointType.Queue))
            throw new ConfigurationException($"Consumers for endpoint {queue} have already been configured.");
        _consumers.Add(new EndpointConsumer(queue, type, registry));
    }

    public EndpointConsumer? Get(string queue, EndpointType type)
        => _consumers.Where(x => x.RoutingKey == queue && x.EndpointType == type).FirstOrDefault();
}
