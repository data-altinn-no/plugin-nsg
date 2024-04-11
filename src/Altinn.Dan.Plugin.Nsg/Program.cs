using Altinn.Dan.Plugin.Nsg.Config;
using Altinn.Dan.Plugin.Nsg.Models;
using Dan.Common.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureDanPluginDefaults()
    .ConfigureAppConfiguration((context, configuration) =>
    {
       // Add more configuration sources if necessary. ConfigureDanPluginDefaults will load environment variables, which includes
       // local.settings.json (if developing locally) and applications settings for the Azure Function
    })
    .ConfigureServices((context, services) =>
    {
        // Add any additional services here
        services.AddMemoryCache();
        services.AddSingleton<ITokenCacheProvider, MemoryTokenCacheProvider>();

        services.AddOptions<ApplicationSettings>()
            .Configure<IConfiguration>((settings, configuration) => configuration.Bind(settings));
    })
    .Build();

await host.RunAsync();
