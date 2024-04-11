using System;
using System.Threading.Tasks;


namespace Altinn.Dan.Plugin.Nsg.Models
{
    public interface ITokenCacheProvider
    {
        public Task<(bool success, TokenResponse result)> TryGetToken(string key);
        public Task Set(string key, TokenResponse value, TimeSpan timeToLive);
    }
}
