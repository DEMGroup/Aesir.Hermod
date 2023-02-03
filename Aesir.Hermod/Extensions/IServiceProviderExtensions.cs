using Aesir.Hermod.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Aesir.Hermod.Extensions;

internal static class IServiceProviderExtensions
{
    internal static ILogger<T> GetLogger<T>(this IServiceProvider sp)
        => sp.GetService<ILogger<T>>() ?? NullLogger<T>.Instance;
}
