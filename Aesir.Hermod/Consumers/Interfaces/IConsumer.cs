using Aesir.Hermod.Messages.Interfaces;

namespace Aesir.Hermod.Consumers.Interfaces;

public interface IConsumer<T> where T : IMessage
{
    Task Consume(IMessageContext<T> message);
}