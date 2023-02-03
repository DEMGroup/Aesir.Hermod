using Aesir.Hermod.Bus.Interfaces;
using Aesir.Hermod.Extensions;
using Aesir.Hermod.Messages.Interfaces;
using Aesir.Hermod.Publishers.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Aesir.Hermod.Bus;

internal class BusWorker : IHostedService
{
    private readonly IMessagingBus _bus;
    private readonly IMessageReceiver _receiver;
    private readonly IMessageProducer _producer;

    public BusWorker(IServiceProvider sp, IMessagingBus bus, IMessageReceiver receiver, IMessageProducer producer)
    {
        _bus = bus;
        _receiver = receiver;
        _producer = producer;
        sp.GetLogger<BusWorker>().LogDebug("Performed initialization of all required services.");
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
