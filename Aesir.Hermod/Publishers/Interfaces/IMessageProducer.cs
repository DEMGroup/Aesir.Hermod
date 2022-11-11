using Aesir.Hermod.Messages.Interfaces;

namespace Aesir.Hermod.Publishers.Interfaces;

internal interface IMessageProducer
{
    void Publish<T>(T message, string? exchange, string? routingKey) where T : IMessage;
}
