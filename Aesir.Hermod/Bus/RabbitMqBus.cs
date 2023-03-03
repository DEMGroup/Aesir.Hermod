using Aesir.Hermod.Bus.Configuration;
using Aesir.Hermod.Bus.Interfaces;
using Aesir.Hermod.Exceptions;
using Aesir.Hermod.Extensions;
using Aesir.Hermod.Publishers.Models;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Collections.Concurrent;

namespace Aesir.Hermod.Bus.Buses;

internal class RabbitMqBus : IMessagingBus
{
    private readonly ILogger<RabbitMqBus> _logger;
    private readonly IModel _model;
    private readonly ConcurrentDictionary<string, ExpectedResponse> ExpectedResponses = new();
    internal RabbitMqBus(IServiceProvider sp, IConnectionFactory connFac)
    {
        _logger = sp.GetLogger<RabbitMqBus>();
        _model = CreateConnectionWithRetry(connFac);
    }

    private static IModel CreateConnectionWithRetry(IConnectionFactory connFac)
    {
        const int maxRetries = 5;
        const int delayMs = 500;

        Exception? lastException = null;

        for (var retryCount = 1; retryCount <= maxRetries; retryCount++)
        {
            try
            {
                return connFac.CreateConnection().CreateModel();
            }
            catch (Exception ex)
            {
                lastException = ex;
                Thread.Sleep(delayMs);
            }
        }
        throw new Exception("Failed to establish connection after multiple attempts.", lastException);
    }

    public IModel GetChannel() => _model;

    public ExpectedResponse? GetExpectedResponse(string correlationId)
    {
        if (!ExpectedResponses.ContainsKey(correlationId)) {
            _logger.LogDebug("Recieved message but no reply was expected.");
            return null;
        }
        _logger.LogDebug("Found expected response for correlation id {id}.", correlationId);
        return ExpectedResponses[correlationId];
    }

    public void RegisterResponseExpected(string correlationId, Action<object?> func, Type resultType)
    {
        if (ExpectedResponses.ContainsKey(correlationId))
            throw new MessagePublishException($"A response is already expected for correlation ID {correlationId}, collision detected.");
        
        _logger.LogDebug("Adding an expected response to correlation id {id}.", correlationId);
        ExpectedResponses.TryAdd(correlationId, new ExpectedResponse(func, resultType));
    }

    public void RemoveCorrelationCallback(string correlationId)
    {
        if (!ExpectedResponses.ContainsKey(correlationId)) {
            _logger.LogDebug("Attempted to remove a reply expectation for correlation id {id} but no reply was expected.", correlationId);
            return;
        }
        _logger.LogDebug("Removing the expectation of a response for correlation id {id}.", correlationId);
        ExpectedResponses.TryRemove(correlationId, out _);
    }
}
