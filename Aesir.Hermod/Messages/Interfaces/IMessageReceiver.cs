namespace Aesir.Hermod.Messages.Interfaces;

internal interface IMessageReceiver
{
    Task InitializeAsync(CancellationToken ct);
}
