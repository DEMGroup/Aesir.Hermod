using Aesir.Hermod.Consumers.Interfaces;
using Aesir.Hermod.Messages.Interfaces;
using Aesir.Hermod.Sample.Messages;

namespace Aesir.Hermod.Sample.Consumers;

internal class SampleMessageConsumer : IConsumer<SampleMessage>
{
    public Task Consume(IMessageContext<SampleMessage> message)
    {
        return Task.CompletedTask;
    }
}
