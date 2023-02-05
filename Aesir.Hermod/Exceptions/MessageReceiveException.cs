namespace Aesir.Hermod.Exceptions;

/// <summary>
/// Represents an error that occured during the publishing of a message
/// </summary>
public class MessageReceiveException : Exception
{
    /// <summary>
    /// Creates a new instance of the <see cref="MessagePublishException"/> class.
    /// </summary>
    /// <param name="message"></param>
    public MessageReceiveException(string message) : base(message) { }

    /// <summary>
    /// Creates a new instance of the <see cref="MessagePublishException"/> class.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public MessageReceiveException(string message, Exception innerException) : base(message, innerException) { }
}
