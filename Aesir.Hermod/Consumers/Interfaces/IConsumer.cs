using Aesir.Hermod.Messages.Interfaces;

namespace Aesir.Hermod.Consumers.Interfaces;

/// <summary>
/// Contains a consume method for processing a single message type
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IConsumer<T> where T : IMessage
{
    /// <summary>
    /// Called when a message of the specified <see cref="IMessage"/> type is received.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    Task Consume(IMessageContext<T> message);
}