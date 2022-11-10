using Aesir.Hermod.Consumers.Interfaces;
using Aesir.Hermod.Extensions;
using System.Collections.ObjectModel;

namespace Aesir.Hermod.Consumers;

internal class ConsumerRegistry : IConsumerRegistry
{
    private readonly List<Type> _consumers = new();
    public ReadOnlyCollection<Type> Consumers { get => _consumers.AsReadOnly(); }
    public bool TryAdd(Type type)
    {
        if (!type.ImplementsGenericInterface(typeof(IConsumer<>))) return false;
        if (_consumers.Any(x => x == type)) return false;

        _consumers.Add(type);
        return true;
    }

    public bool TryGet<T>(out Type? consumer) where T : class
    {
        consumer = null;
        if (!typeof(T).ImplementsGenericInterface(typeof(IConsumer<>))) return false;

        var type = typeof(T);
        consumer = _consumers.Where(c => c == type).FirstOrDefault();
        return consumer != null;
    }
}
