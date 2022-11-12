using Aesir.Hermod.Consumers.Interfaces;
using Aesir.Hermod.Extensions;
using Aesir.Hermod.Messages.Interfaces;

namespace Aesir.Hermod.Tests.Extensions;

public class TypeExtensionsTests
{
    [Fact]
    public void ImplementsGenericInterface_ShouldDetectIConsumer()
    {
        var res = typeof(TestConsumer).ImplementsGenericInterface(typeof(IConsumer<>));
        Assert.True(res);
    }

    [Fact]
    public void ImplementsGenericInterface_ShouldReturnFalseForNonIConsumer()
    {
        var res = typeof(TestConsumer).ImplementsGenericInterface(typeof(string));
        Assert.False(res);
    }
}

internal record TestMessage() : IMessage;
internal record TestConsumer() : IConsumer<TestMessage>
{
    public Task Consume(IMessageContext<TestMessage> message)
    {
        throw new NotImplementedException();
    }
}
