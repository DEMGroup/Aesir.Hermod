using Aesir.Hermod.Bus.Buses;
using Aesir.Hermod.Bus.Configuration;
using Aesir.Hermod.Exceptions;
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
        var model = new Mock<IModel>();

        connection.Setup(c => c.CreateModel()).Returns(model.Object);
        connFactory.Setup(c => c.CreateConnection()).Returns(connection.Object);

        var bus = new RabbitMqBus(new BusOptions(), connFactory.Object);
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
