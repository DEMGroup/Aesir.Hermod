using Aesir.Hermod.Bus.Buses;
using Aesir.Hermod.Bus.Configuration;
using Aesir.Hermod.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RabbitMQ.Client;

namespace Aesir.Hermod.Tests.Bus;

public class RabbitMqBusTests
{
    private const string CORRELATION_ID = "1234";
    private static RabbitMqBus CreateBus()
    {
        var connFactory = new Mock<IConnectionFactory>();
        var connection = new Mock<IConnection>();
        var channel = new Mock<IChannel>();

        connection.Setup(c => c.CreateChannelAsync(It.IsAny<CreateChannelOptions>(), It.IsAny<CancellationToken>())).ReturnsAsync(channel.Object);
        connFactory.Setup(c => c.CreateConnectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(connection.Object);

        var bus = new RabbitMqBus(new ServiceCollection().BuildServiceProvider(), connFactory.Object);
        return bus;
    }

    [Fact]
    public void RegisterResponseExpected_ShouldRegisterFirstTime()
    {
        var bus = CreateBus();
        bus.RegisterResponseExpected(CORRELATION_ID, obj => { }, typeof(string));

        var res = bus.GetExpectedResponse(CORRELATION_ID);
        Assert.NotNull(res);
    }

    [Fact]
    public void RegisterResponseExpected_ShouldThrowOnDuplicate()
    {
        var bus = CreateBus();
        bus.RegisterResponseExpected(CORRELATION_ID, obj => { }, typeof(string));
        Assert.Throws<MessagePublishException>(() => bus.RegisterResponseExpected(CORRELATION_ID, obj => { }, typeof(string)));
    }

    [Fact]
    public void RemoveCorrelationCallback_ShouldRemoveExpectedResponse()
    {
        var bus = CreateBus();
        bus.RegisterResponseExpected(CORRELATION_ID, obj => { }, typeof(string));
        bus.RemoveCorrelationCallback(CORRELATION_ID);

        var res = bus.GetExpectedResponse(CORRELATION_ID);
        Assert.Null(res);
    }
}
