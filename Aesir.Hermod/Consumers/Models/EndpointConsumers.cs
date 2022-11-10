using Aesir.Hermod.Bus.Enums;

namespace Aesir.Hermod.Consumers.Models;
public class EndpointConsumer {
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
