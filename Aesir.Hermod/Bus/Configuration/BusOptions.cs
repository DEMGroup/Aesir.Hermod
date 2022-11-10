namespace Aesir.Hermod.Bus.Configuration;
public class BusOptions
{
    public string Host { get; set; }
    public ushort Port { get; set; }
    public string VHost { get; set; }
    public string User { get; set; }
    public string Pass { get; set; }

    public BusOptions()
    {
        Host = "localhost";
        Port = 5672;
        VHost = "/";
        User = "guest";
        Pass = "guest";
    }
}
