namespace Aesir.Hermod.Messages.Interfaces;

/// <summary>
/// Represents the result to a received message
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IMessageResult<T> where T : IMessage { }

