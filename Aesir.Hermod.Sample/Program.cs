using Aesir.Hermod.Extensions;
using Aesir.Hermod.Sample;
using Aesir.Hermod.Sample.Consumers;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHermod(conf =>
        {
            conf.ConfigureHost(opts =>
            {
                opts.Host = "localhost";
                opts.Port = 5672;
                opts.VHost = "/";
                opts.User = "guest";
                opts.Pass = "guest";
            });
        });
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
