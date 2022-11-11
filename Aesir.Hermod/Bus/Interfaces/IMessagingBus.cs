using Aesir.Hermod.Publishers.Models;
using RabbitMQ.Client;

namespace Aesir.Hermod.Bus.Interfaces;

/// <summary>
/// Contains the currently live RabbitMQ connection and relevant data.
/// </summary>
internal interface IMessagingBus
{
    IModel GetChannel();
    void RegisterResponseExpected<T>(string correlationId, Action<object> func, Type ExpectedResult);
    ExpectedResponse? GetExpectedResponse(string correlationId);
    void RemoveCorrelationCallback(string correlationId);
}
