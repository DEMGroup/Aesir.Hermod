namespace Aesir.Hermod.Models;

public record QueueDeclaration(string Queue, bool Durable, bool AutoDelete, bool Exclusive);