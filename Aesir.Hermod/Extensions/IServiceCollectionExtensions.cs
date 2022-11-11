using Aesir.Hermod.Bus;
using Aesir.Hermod.Bus.Interfaces;
using Aesir.Hermod.Configuration;
using Aesir.Hermod.Configuration.Interfaces;
using Aesir.Hermod.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Aesir.Hermod.Extensions;

/// <summary>
/// Contains registration extensions used to configure and enable Hermod.
/// </summary>
public static class IServiceCollectionExtensions
{

    /// <summary>
    /// Configures Hermod and add its to the DI container <paramref name="services"/> and exposes configuration methods.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    /// <exception cref="ConfigurationException"></exception>
    public static IServiceCollection AddHermod(this IServiceCollection services, Action<IConfigurationBuilder>? configure = null)
    {
        if (services.Any(x => x.ServiceType == typeof(IMessagingBus)))
            throw new ConfigurationException($"{nameof(AddHermod)}() has already been called and can only be called once.");

        var builder = new ConfigurationBuilder();
        configure?.Invoke(builder);

        services.AddSingleton(sp => builder.ConfigureBus());
        services.AddSingleton(sp => builder.ConfigureReceiver(sp));
        services.AddSingleton(sp => builder.ConfigureProducer(sp));

        services.AddHostedService<BusWorker>();
        return services;
    }
}
