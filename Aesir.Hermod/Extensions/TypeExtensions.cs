namespace Aesir.Hermod.Extensions;

internal static class TypeExtensions
{
    public static bool ImplementsGenericInterface(this Type type, Type targetType)
        => type.Equals(targetType) ||
            (type.IsGenericType && type.GetGenericTypeDefinition().Equals(targetType)) ||
            type.GetInterfaces().Any(i => i.IsGenericType && i.ImplementsGenericInterface(targetType));
}
