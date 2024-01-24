using Aesir.Hermod.Bus.Enums;
using Aesir.Hermod.Consumers;
using Aesir.Hermod.Exceptions;
using Aesir.Hermod.Messages.Interfaces;
using Aesir.Hermod.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Aesir.Hermod.Tests.Consumers;

public class EndpointConsumerFactoryTests
{
    private readonly QueueDeclaration _queue = new ("TestQueue", true, false, false);

    [Fact]
    public void Add_ShouldAddConsumer()
    {
        var consumerFac = new EndpointConsumerFactory();
        consumerFac.AddQueue(
            _queue,
            new ConsumerRegistry(new ServiceCollection().BuildServiceProvider())
        );
        Assert.Single(consumerFac.GetQueues());
        Assert.Empty(consumerFac.GetExchanges());
    }

    [Fact]
    public void Add_ShouldThrowWhenRegisteringTheSameEndpointTwice()
    {
        var consumerFac = new EndpointConsumerFactory();
        consumerFac.AddQueue(_queue,
            new ConsumerRegistry(new ServiceCollection().BuildServiceProvider()));
        Assert.Throws<ConfigurationException>(() => consumerFac.AddQueue(_queue,
            new ConsumerRegistry(new ServiceCollection().BuildServiceProvider())));
    }

    [Fact]
    public void Get_ShouldReturnEndpoint()
    {
        var consumerFac = new EndpointConsumerFactory();
        consumerFac.AddQueue(_queue,
            new ConsumerRegistry(new ServiceCollection().BuildServiceProvider()));

        var res = consumerFac.Get(_queue.Queue, EndpointType.Queue);
        Assert.NotNull(res);
    }
}

internal record Message(string Msg) : IMessage;