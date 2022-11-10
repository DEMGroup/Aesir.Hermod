namespace Aesir.Hermod.Consumers.Interfaces;

public interface IConsumerFactory
{
    void RegisterConsumer(Type type);
}
