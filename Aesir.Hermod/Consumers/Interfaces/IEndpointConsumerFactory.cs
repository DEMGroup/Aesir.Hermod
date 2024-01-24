using Aesir.Hermod.Bus.Enums;
using Aesir.Hermod.Consumers.Models;
using Aesir.Hermod.Models;

namespace Aesir.Hermod.Consumers.Interfaces;

internal interface IEndpointConsumerFactory
{
    void AddQueue(QueueDeclaration queue, ConsumerRegistry registry);
    void AddExchange(ExchangeDeclaration queue, ConsumerRegistry registry);
    EndpointConsumer? Get(string queue, EndpointType type);
    IEnumerable<QueueDeclaration> GetQueues();
    IEnumerable<ExchangeDeclaration> GetExchanges();
}
