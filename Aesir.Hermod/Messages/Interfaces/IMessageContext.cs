namespace Aesir.Hermod.Messages.Interfaces;

/// <summary>
/// Contains a received message and it's relevant context methods/properties
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IMessageContext<T> where T : IMessage
{
    /// <summary>
    /// The message received by the client
    /// </summary>
    T? Message { get; }

    /// <summary>
    /// Responds in the provided type to the sender, if the receiver doesn't have the specified type null will be returned.
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="message"></param>
    void Respond<TResult>(TResult message) where TResult : IMessageResult<T>;
}
