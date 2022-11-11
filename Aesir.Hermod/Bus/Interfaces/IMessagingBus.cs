using Aesir.Hermod.Messages.Interfaces;

namespace Aesir.Hermod.Bus.Interfaces;

/// <summary>
/// Contains the currently live RabbitMQ connection and relevant data.
/// </summary>
public interface IMessagingBus
{
    void Publish<T>(T message, string? exchange, string? routingKey) where T : IMessage;
}
