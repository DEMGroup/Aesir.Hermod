using Aesir.Hermod.Messages.Interfaces;
using Aesir.Hermod.Models;

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
    /// <param name="configure"></param>
    /// <param name="routingKey"></param>
    void SendToExchange<T>(
        T message,
        ExchangeDeclaration configure,
        string? routingKey = null) where T : IMessage;

    /// <summary>
    /// Sends a message to the specified queue.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="message"></param>
    /// <param name="configure"></param>
    void Send<T>(
        T message, 
        QueueDeclaration configure) where T : IMessage;

    /// <summary>
    /// Sends a message to a single queue and then waits for a response of type <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <param name="message"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    Task<TResult?> SendWithResponseAsync<TResult, T>(
        T message,
        QueueDeclaration configure) where T : IMessage where TResult : IMessageResult<T>;
}