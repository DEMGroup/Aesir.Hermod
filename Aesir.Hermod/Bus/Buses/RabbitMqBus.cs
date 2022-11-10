using Aesir.Hermod.Bus.Interfaces;
using Aesir.Hermod.Consumers;
using Aesir.Hermod.Consumers.Interfaces;

namespace Aesir.Hermod.Bus.Buses;

public class RabbitMqBus : IMessagingBus
{
    private readonly IConsumerRegistry ConsumerRegistry;
    public RabbitMqBus()
    {
        ConsumerRegistry = new ConsumerRegistry();
    }
}
