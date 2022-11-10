using Aesir.Hermod.Bus.Enums;

namespace Aesir.Hermod.Consumers.Models;
internal record EndpointConsumer(string RoutingKey, EndpointType EndpointType, ConsumerRegistry Registry);
