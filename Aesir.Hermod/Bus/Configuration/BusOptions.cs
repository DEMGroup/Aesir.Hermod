namespace Aesir.Hermod.Bus.Configuration;

/// <summary>
/// Data class containing info required to configure the underlying RabbitMQ connection.
/// </summary>
public class BusOptions
{
    /// <summary>
    /// Host url for the RabbitMQ instance.
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    /// Host port for the RabbitMQ instance.
    /// </summary>
    public ushort Port { get; set; }

    /// <summary>
    /// Vhost to use for the RabbitMQ instance.
    /// </summary>
    public string VHost { get; set; }

    /// <summary>
    /// Username for logging into RabbitMQ instance.
    /// </summary>
    public string User { get; set; }

    /// <summary>
    /// Password for logging into the RabbitMQ instance.
    /// </summary>
    public string Pass { get; set; }

    /// <summary>
    /// If the connection is lost, should it be retried. Default of true.
    /// </summary>
    public bool RetryConnection { get; set; } = true;

    /// <summary>
    /// The time in seconds between connection retry attempts. Default of 5 seconds.
    /// </summary>
    public int RetryWaitTime { get; set; } = 5;

    /// <summary>
    /// Creates a new instance of the <see cref="BusOptions"/> class and populates the default values.
    /// </summary>
    public BusOptions()
    {
        Host = "localhost";
        Port = 5672;
        VHost = "/";
        User = "guest";
        Pass = "guest";
    }
}
