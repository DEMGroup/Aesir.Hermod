namespace Aesir.Hermod.Exceptions;

/// <summary>
/// Represents an error that occured during the configuration of Hermod
/// </summary>
public class ConfigurationException : Exception
{
    /// <summary>
    /// Creates a new instance of the <see cref="ConfigurationException"/> class.
    /// </summary>
    /// <param name="message"></param>
    public ConfigurationException(string message) : base(message) { }

    /// <summary>
    /// Creates a new instance of the <see cref="ConfigurationException"/> class.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="inner"></param>
    public ConfigurationException(string message, Exception inner) : base(message, inner) { }

    /// <summary>
    /// Creates a new instance of the <see cref="ConfigurationException"/> class.
    /// </summary>
    public ConfigurationException() : base()
    {
    }
}
