namespace Aesir.Hermod.Publishers.Configuration;

/// <summary>
/// Contains properties to configure the producer.
/// </summary>
public class ProducerOptions
{
    /// <summary>
    /// How long to wait for a message response before throwing a timeout error.
    /// </summary>
    public TimeSpan ResponseTimeout { get; set; }

    /// <summary>
    /// Creates a new instance of the <see cref="ProducerOptions"/> class.
    /// </summary>
    public ProducerOptions()
    {
        ResponseTimeout = TimeSpan.FromSeconds(10);
    }
}
