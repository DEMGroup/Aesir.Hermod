using Aesir.Hermod.Bus.Interfaces;
using Aesir.Hermod.Configuration;
using Aesir.Hermod.Configuration.Interfaces;
using Aesir.Hermod.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace Aesir.Hermod.Extensions;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection UseHermod(this IServiceCollection services, Action<IConfigurationBuilder>? configure = null)
    {
        if (services.Any(x => x.ServiceType == typeof(IMessagingBus)))
            throw new ConfigurationException($"{nameof(UseHermod)}() has already been called and can only be called once.");


        var builder = new ConfigurationBuilder();
        configure?.Invoke(builder);

        //services.AddSingleton(sp => builder.ConfigureBus(sp));

        return services;
    }
}
