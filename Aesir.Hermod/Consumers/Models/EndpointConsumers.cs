using Aesir.Hermod.Bus.Enums;

namespace Aesir.Hermod.Consumers.Models;

/// <summary>
/// Contains consumer registration information relating to a specific endpoint.
/// </summary>
public class EndpointConsumer<TDeclaration>
{
    internal string RoutingKey { get; }
    internal TDeclaration Declaration { get; }
    internal EndpointType EndpointType { get; }
    internal ConsumerRegistry Registry { get; }

    internal EndpointConsumer(
        string routingKey,
        EndpointType endpointType,
        ConsumerRegistry registry,
        TDeclaration declaration)
    {
        RoutingKey = routingKey;
        EndpointType = endpointType;
        Registry = registry;
        Declaration = declaration;
    }
}

public class EndpointConsumer
{
    internal string RoutingKey { get; }
    internal EndpointType EndpointType { get; }
    internal ConsumerRegistry Registry { get; }

    internal EndpointConsumer(
        string routingKey,
        EndpointType endpointType,
        ConsumerRegistry registry)
    {
        RoutingKey = routingKey;
        EndpointType = endpointType;
        Registry = registry;
    }

    internal static EndpointConsumer FromTypedConsumer<TDeclaration>(EndpointConsumer<TDeclaration> consumer)
        => new (consumer.RoutingKey, consumer.EndpointType, consumer.Registry);
}