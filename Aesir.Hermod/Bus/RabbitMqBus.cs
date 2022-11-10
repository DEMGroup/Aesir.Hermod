using Aesir.Hermod.Bus.Configuration;
using Aesir.Hermod.Bus.Interfaces;
using Aesir.Hermod.Consumers.Interfaces;
using RabbitMQ.Client;

namespace Aesir.Hermod.Bus.Buses;

internal class RabbitMqBus : IMessagingBus
{
    private readonly IModel _model;
    private readonly IEndpointConsumerFactory _consumers;
    private readonly IServiceProvider _serviceProvider;
    internal RabbitMqBus(BusOptions opts, IEndpointConsumerFactory endpointConsumerFac, IServiceProvider sp)
    {
        var connFactory = new ConnectionFactory
        {
            UserName = opts.User,
            Password = opts.Pass,
            HostName = opts.Host,
            Port = opts.Port,
            VirtualHost = opts.VHost
        };
        _model = connFactory.CreateConnection().CreateModel();
        _consumers = endpointConsumerFac;
        _serviceProvider = sp;
    }
}
