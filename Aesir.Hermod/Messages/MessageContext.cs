using Aesir.Hermod.Messages.Interfaces;
using RabbitMQ.Client.Events;

namespace Aesir.Hermod.Messages;

internal class MessageContext<T> : IMessageContext<T> where T : IMessage
{
    public T? Message { get; set; }
    public MessageContext(T message, BasicDeliverEventArgs _)
    {
        Message = message;
    }

    public void Respond()
    {
        throw new NotImplementedException();
    }
}
