using Aesir.Hermod.Extensions;
using Aesir.Hermod.Sample;
using Aesir.Hermod.Sample.Consumers;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.UseHermod(conf =>
        {
            conf.Host(opts =>
            {
                opts.Host = "localhost";
                opts.Port = 5672;
                opts.VHost = "/";
                opts.User = "guest";
                opts.Pass = "guest";
            });

            conf.ConsumeQueue("test-queue", x =>
            {
                x.RegisterConsumer(typeof(TestMessageConsumer));
            });

            conf.ConsumeExchange("test-exchange", x =>
            {
                x.RegisterConsumer(typeof(TestMessageConsumer));
            });
        });
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
