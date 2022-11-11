namespace Aesir.Hermod.Messages.Interfaces;

internal interface IMessageReceiver
{
    void CreateConsumer(string queue);
}
