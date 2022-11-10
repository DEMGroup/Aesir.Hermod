using RabbitMQ.Client.Events;

namespace Aesir.Hermod.Messages.Interfaces;

internal interface IMessageReceiver
{
    void CreateConsumer(string queue);
}
