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

    void Respond<TResult>(TResult message) where TResult : IMessageResult<T>;
}
