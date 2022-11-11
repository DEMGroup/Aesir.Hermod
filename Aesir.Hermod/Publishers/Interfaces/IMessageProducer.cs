using Aesir.Hermod.Messages.Interfaces;

namespace Aesir.Hermod.Publishers.Interfaces;

public interface IMessageProducer
{
    void SendToExchange<T>(T message, string exchange) where T : IMessage;
    void Send<T>(T message, string queue) where T : IMessage;
    Task<TResult> SendWithResponseAsync<TResult, T>(T message, string? queue) where T : IMessage where TResult : IMessageResult<T>;
    public void Respond<TResult, T>(TResult message, string correlationId, string replyTo) where T : IMessage where TResult : IMessageResult<T>;
}
