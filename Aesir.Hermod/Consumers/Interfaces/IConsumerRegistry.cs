using System.Collections.ObjectModel;

namespace Aesir.Hermod.Consumers.Interfaces;

internal interface IConsumerRegistry
{
    ReadOnlyCollection<Type> Consumers { get; }
    bool TryGet<T>(out Type? consumer) where T : class;
    bool TryAdd(Type type);
}
