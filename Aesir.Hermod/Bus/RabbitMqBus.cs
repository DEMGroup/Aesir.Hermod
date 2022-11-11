using Aesir.Hermod.Bus.Configuration;
using Aesir.Hermod.Bus.Interfaces;
using Aesir.Hermod.Exceptions;
using Aesir.Hermod.Publishers.Models;
using RabbitMQ.Client;
using System.Collections.Concurrent;

namespace Aesir.Hermod.Bus.Buses;

internal class RabbitMqBus : IMessagingBus
{
    private readonly IModel _model;
    private readonly ConcurrentDictionary<string, ExpectedResponse> ExpectedResponses = new();
    internal RabbitMqBus(BusOptions opts)
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
    }

    public IModel GetChannel() => _model;

    public ExpectedResponse? GetExpectedResponse(string correlationId)
    {
        if (!ExpectedResponses.ContainsKey(correlationId)) return null;
        return ExpectedResponses[correlationId];
    }

    public void RegisterResponseExpected<T>(string correlationId, Action<object> func, Type resultType)
    {
        if (ExpectedResponses.ContainsKey(correlationId))
            throw new MessagePublishException($"A response is already expected for correlation ID {correlationId}, collision detected.");
        ExpectedResponses.TryAdd(correlationId, new ExpectedResponse(func, resultType));
    }

    public void RemoveCorrelationCallback(string correlationId)
    {
        if (!ExpectedResponses.ContainsKey(correlationId)) return;
        ExpectedResponses.TryRemove(correlationId, out _);
    }
}
