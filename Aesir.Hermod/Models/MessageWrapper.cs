namespace Aesir.Hermod.Models;

/// <summary>
/// Contains the serialised message and it's type, used to wrap messages before sending
/// </summary>
public class MessageWrapper
{
    /// <summary>
    /// Serialised message
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// The type of the serialized message
    /// </summary>
    public string Type { get; set; } = null!;

    /// <summary>
    /// Creates a new instance of the <see cref="MessageWrapper"/> class.
    /// </summary>
    public MessageWrapper() { }
}
