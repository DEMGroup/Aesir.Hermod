using System.Collections.ObjectModel;

namespace Aesir.Hermod.Consumers.Interfaces;

public interface IConsumerRegistry
{
    ReadOnlyCollection<Type> Consumers { get; }
    void RegisterConsumer(Type type);
}
