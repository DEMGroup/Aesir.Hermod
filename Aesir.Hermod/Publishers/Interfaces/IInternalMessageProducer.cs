using Aesir.Hermod.Messages.Interfaces;

namespace Aesir.Hermod.Publishers.Interfaces;

internal interface IInternalMessageProducer : IMessageProducer
{
    /// <summary>
    /// Responds to a provided queue.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <param name="message"></param>
    /// <param name="correlationId"></param>
    /// <param name="replyTo"></param>
    internal void Respond<TResult, T>(TResult message, string correlationId, string replyTo)
        where T : IMessage where TResult : IMessageResult<T>;

    /// <summary>
    /// Sends an empty message to act as a receipt of a message being processed.
    /// </summary>
    /// <param name="correlationId"></param>
    internal void SendEmpty(string correlationId);
}