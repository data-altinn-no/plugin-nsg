using System;

namespace Altinn.Dan.Plugin.Nsg.Config
{
    public interface IApplicationSettings
    {
        TimeSpan Breaker_OpenCircuitTime { get; }
    }
}
