using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Altinn.Dan.Plugin.Nsg.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nadobe.Common.Interfaces;
using Polly;
using Polly.Extensions.Http;
using Polly.Registry;

namespace Altinn.Dan.Plugin.Nsg
{
    class Program
    {
        private static IApplicationSettings ApplicationSettings { get; set; }

        private static Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(services =>
                {
                    services.AddLogging();
                    services.AddHttpClient();

                    services.AddSingleton<IApplicationSettings, ApplicationSettings>();
                    services.AddSingleton<IEvidenceSourceMetadata, EvidenceSourceMetadata>();

                    ApplicationSettings = services.BuildServiceProvider().GetRequiredService<IApplicationSettings>();

                    var registry = new PolicyRegistry()
                    {
                        { "defaultCircuitBreaker", HttpPolicyExtensions.HandleTransientHttpError().CircuitBreakerAsync(4, ApplicationSettings.Breaker_OpenCircuitTime) },
                    };
                    services.AddPolicyRegistry(registry);

                    // Client configured with circuit breaker policies
                    services.AddHttpClient("SafeHttpClient", client => { client.Timeout = new TimeSpan(0, 0, 30); })
                        .AddPolicyHandlerFromRegistry("defaultCircuitBreaker");

                    services.Configure<JsonSerializerOptions>(options =>
                    {
                        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                        options.Converters.Add(new JsonStringEnumConverter());
                    });
                })
                .Build();
            return host.RunAsync();
        }
    }
}
