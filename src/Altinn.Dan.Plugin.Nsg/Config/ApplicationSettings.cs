using System;

namespace Altinn.Dan.Plugin.Nsg.Config
{
    public class ApplicationSettings : IApplicationSettings
    {
        public static ApplicationSettings ApplicationConfig;

        public ApplicationSettings()
        {
            ApplicationConfig = this;
        }
        public TimeSpan Breaker_OpenCircuitTime => TimeSpan.FromSeconds(int.Parse(Environment.GetEnvironmentVariable("Breaker_OpenCircuitTime") ?? "0"));
    }
}
