using Aesir.Hermod.Bus.Enums;
using Aesir.Hermod.Consumers;
using Aesir.Hermod.Exceptions;
using Aesir.Hermod.Messages.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Aesir.Hermod.Tests.Consumers;

public class EndpointConsumerFactoryTests
{
    private const string QUEUE = "1234";
    

    [Fact]
    public void Add_ShouldAddConsumer()
    {
        var consumerFac = new EndpointConsumerFactory();
        consumerFac.Add(QUEUE, EndpointType.Queue, new ConsumerRegistry(new ServiceCollection().BuildServiceProvider()));
        Assert.Single(consumerFac.GetEndpoints());
    }

    [Fact]
    public void Add_ShouldThrowWhenRegisteringTheSameEndpointTwice()
    {
        var consumerFac = new EndpointConsumerFactory();
        consumerFac.Add(QUEUE, EndpointType.Queue, new ConsumerRegistry(new ServiceCollection().BuildServiceProvider()));
        Assert.Throws<ConfigurationException>(() => consumerFac.Add(QUEUE, EndpointType.Queue, new ConsumerRegistry(new ServiceCollection().BuildServiceProvider())));
    }

    [Fact]
    public void Add_ShouldAllowSameNameButDifferentTypes()
    {
        var consumerFac = new EndpointConsumerFactory();
        consumerFac.Add(QUEUE, EndpointType.Queue, new ConsumerRegistry(new ServiceCollection().BuildServiceProvider()));
        consumerFac.Add(QUEUE, EndpointType.Exchange, new ConsumerRegistry(new ServiceCollection().BuildServiceProvider()));

        var endpoints = consumerFac.GetEndpoints();
        Assert.Equal(2, endpoints.Count());
    }

    [Fact]
    public void Get_ShouldReturnEndpoint()
    {
        var consumerFac = new EndpointConsumerFactory();
        consumerFac.Add(QUEUE, EndpointType.Queue, new ConsumerRegistry(new ServiceCollection().BuildServiceProvider()));

        var res = consumerFac.Get(QUEUE, EndpointType.Queue);
        Assert.NotNull(res);
    }
}

internal record Message(string Msg) : IMessage;