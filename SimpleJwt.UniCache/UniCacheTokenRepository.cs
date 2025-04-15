using System;
using System.Threading;
using System.Threading.Tasks;
using SimpleJwt.Abstractions.Caching;
using UniCache;
using UniCache.Encryption;

namespace SimpleJwt.UniCache
{
    /// <summary>
    /// A UniCache-based implementation of ITokenCacheStorage for persistent and encrypted JWT token storage.
    /// </summary>
    public class UniCacheTokenRepository : ITokenCacheStorage
    {
        // TODO: Implement UniCache-backed token storage, including encryption and persistence.
        // This is a stub for further development.
        public Task SetTokenAsync(string key, IJwtToken token, TimeSpan? expires = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IJwtToken> GetTokenAsync(string key, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task RemoveTokenAsync(string key, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task ClearAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
