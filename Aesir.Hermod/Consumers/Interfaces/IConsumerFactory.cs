namespace Aesir.Hermod.Consumers.Interfaces;

/// <summary>
/// Used during the configuration pipeline to register <see cref="IConsumer{T}"/>'s.
/// </summary>
public interface IConsumerFactory
{
    /// <summary>
    /// Inserts the provided type into a list of Consumers, no duplicate checking is done at this point.
    /// </summary>
    /// <param name="type"></param>
    void RegisterConsumer(Type type);
}
