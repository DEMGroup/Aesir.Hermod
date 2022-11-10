using Aesir.Hermod.Consumers.Interfaces;
using Aesir.Hermod.Messages.Interfaces;
using Aesir.Hermod.Sample.Messages;

namespace Aesir.Hermod.Sample.Consumers;

internal class TestMessageConsumer : IConsumer<TestMessage>
{
    public Task Consume(IMessageContext<TestMessage> message)
    {
        throw new NotImplementedException();
    }
}
