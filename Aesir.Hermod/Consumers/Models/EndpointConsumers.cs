using Aesir.Hermod.Bus.Enums;

namespace Aesir.Hermod.Consumers.Models;

/// <summary>
/// Contains consumer registration information relating to a specific endpoint.
/// </summary>
public class EndpointConsumer<TDeclaration>
{
    internal string Name { get; }
    internal string? RoutingKey { get; }
    internal TDeclaration Declaration { get; }
    internal EndpointType EndpointType { get; }
    internal ConsumerRegistry Registry { get; }

    internal EndpointConsumer(
        string name,
        EndpointType endpointType,
        ConsumerRegistry registry,
        TDeclaration declaration,
        string? routingKey = null)
    {
        Name = name;
        EndpointType = endpointType;
        Registry = registry;
        Declaration = declaration;
        RoutingKey = endpointType == EndpointType.Exchange ? routingKey : null;
    }
}

public class EndpointConsumer
{
    internal string Name { get; }
    internal EndpointType EndpointType { get; }
    internal ConsumerRegistry Registry { get; }
    internal string? RoutingKey { get; }

    internal EndpointConsumer(
        string name,
        EndpointType endpointType,
        ConsumerRegistry registry,
        string? routingKey)
    {
        Name = name;
        EndpointType = endpointType;
        Registry = registry;
        RoutingKey = routingKey;
    }

    internal static EndpointConsumer FromTypedConsumer<TDeclaration>(EndpointConsumer<TDeclaration> consumer)
        => new (consumer.Name, consumer.EndpointType, consumer.Registry, consumer.RoutingKey);
}