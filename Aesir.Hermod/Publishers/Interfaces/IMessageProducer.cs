using Aesir.Hermod.Messages.Interfaces;

namespace Aesir.Hermod.Publishers.Interfaces;

/// <summary>
/// Contains all methods for sending messages
/// </summary>
public interface IMessageProducer
{
    /// <summary>
    /// Sends a message to the specified exchange.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="message"></param>
    /// <param name="exchange"></param>
    void SendToExchange<T>(T message, string exchange) where T : IMessage;

    /// <summary>
    /// Sends a message to the specified queue.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="message"></param>
    /// <param name="queue"></param>
    void Send<T>(T message, string queue) where T : IMessage;

    /// <summary>
    /// Sends a message to a single queue and then waits for a response of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <param name="message"></param>
    /// <param name="queue"></param>
    /// <returns></returns>
    Task<TResult> SendWithResponseAsync<TResult, T>(T message, string? queue) where T : IMessage where TResult : IMessageResult<T>;

    /// <summary>
    /// Responds to a provided queue.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <param name="message"></param>
    /// <param name="correlationId"></param>
    /// <param name="replyTo"></param>
    internal void Respond<TResult, T>(TResult message, string correlationId, string replyTo) where T : IMessage where TResult : IMessageResult<T>;
}
