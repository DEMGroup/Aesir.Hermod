using Aesir.Hermod.Bus.Enums;

namespace Aesir.Hermod.Consumers.Models;

/// <summary>
/// Contains consumer registration information relating to a specific endpoint.
/// </summary>
public class EndpointConsumer
{
    internal string RoutingKey { get; set; }
    internal EndpointType EndpointType { get; set; }
    internal ConsumerRegistry Registry { get; set; }

    internal EndpointConsumer(string routingKey, EndpointType endpointType, ConsumerRegistry registry)
    {
        RoutingKey = routingKey;
        EndpointType = endpointType;
        Registry = registry;
    }
}
