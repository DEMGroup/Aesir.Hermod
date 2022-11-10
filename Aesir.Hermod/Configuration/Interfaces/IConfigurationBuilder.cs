using Aesir.Hermod.Bus.Configuration;
using Aesir.Hermod.Consumers.Interfaces;

namespace Aesir.Hermod.Configuration.Interfaces;

public interface IConfigurationBuilder
{
    void Host(Action<BusOptions> opts);
    void ConsumeQueue(string queue, Action<IConsumerFactory> configure);
    void ConsumeExchange(string queue, Action<IConsumerFactory> configure);
}
