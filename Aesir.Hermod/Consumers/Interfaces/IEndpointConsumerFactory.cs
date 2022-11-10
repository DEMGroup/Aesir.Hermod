using Aesir.Hermod.Bus.Enums;
using Aesir.Hermod.Consumers.Models;

namespace Aesir.Hermod.Consumers.Interfaces;

internal interface IEndpointConsumerFactory
{
    void Add(string queue, EndpointType type, ConsumerRegistry registry);
    EndpointConsumer? Get(string queue, EndpointType type);
    IEnumerable<(string, EndpointType)> GetEndpoints();
}
