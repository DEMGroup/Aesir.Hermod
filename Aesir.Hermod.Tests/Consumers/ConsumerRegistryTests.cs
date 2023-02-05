using Aesir.Hermod.Consumers;
using Aesir.Hermod.Consumers.Interfaces;
using Aesir.Hermod.Exceptions;
using Aesir.Hermod.Messages.Interfaces;

namespace Aesir.Hermod.Tests.Consumers;

public class ConsumerRegistryTests
{
    [Fact]
    public void TryAdd_ShouldAddFirstTime()
    {
        var consumerReg = new ConsumerRegistry();
        var success = consumerReg.TryAdd(typeof(string));
        Assert.True(success);
    }

    [Fact]
    public void TryAdd_ShouldFailForDuplicate()
    {
        var consumerReg = new ConsumerRegistry();
        consumerReg.TryAdd(typeof(string));
        var success = consumerReg.TryAdd(typeof(string));
        Assert.False(success);
    }

    [Fact]
    public void RegisterConsumer_ShouldThrowWhenNotIConsumer()
    {
        var consumerReg = new ConsumerRegistry();
        Assert.Throws<ConfigurationException>(() => consumerReg.RegisterConsumer(typeof(string)));
    }

    [Fact]
    public void RegisterConsumer_ShouldThrowWhenDuplicate()
    {
        var consumerReg = new ConsumerRegistry();
        consumerReg.RegisterConsumer(typeof(TestConsumer));
        Assert.Throws<ConfigurationException>(() => consumerReg.RegisterConsumer(typeof(TestConsumer)));
    }

    [Fact]
    public void Find_ShouldGetTheConsumerParameterType()
    {
        var consumerReg = new ConsumerRegistry();
        consumerReg.RegisterConsumer(typeof(TestConsumer));
        var res = consumerReg.Find(nameof(TestMessage));
        Assert.NotNull(res);
    }

    [Fact]
    public void Find_ShouldReturnNullIfNoConsumerFound()
    {
        var consumerReg = new ConsumerRegistry();
        var res = consumerReg.Find(nameof(TestConsumer));
        Assert.Null(res);
    }
}

internal record TestMessage(): IMessage;
internal record TestConsumer() : IConsumer<TestMessage>
{
    public Task Consume(IMessageContext<TestMessage> message)
    {
        throw new NotImplementedException();
    }
}
