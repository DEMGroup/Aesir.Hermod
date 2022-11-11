namespace Aesir.Hermod.Publishers.Configuration;

public class ProducerOptions
{
    /// <summary>
    /// How long to wait for a message response
    /// </summary>
    public TimeSpan ResponseTimeout { get; set; }

    public ProducerOptions()
    {
        ResponseTimeout = TimeSpan.FromSeconds(10);
    }
}
