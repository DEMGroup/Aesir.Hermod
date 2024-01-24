namespace Aesir.Hermod.Models;

public record ExchangeDeclaration(
    string Exchange,
    string Type,
    bool Durable,
    bool AutoDelete,
    IDictionary<string, object>? Arguments);