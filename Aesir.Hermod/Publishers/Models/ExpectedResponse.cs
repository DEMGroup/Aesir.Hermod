namespace Aesir.Hermod.Publishers.Models;

internal record ExpectedResponse(Action<object> Action, Type Type);
