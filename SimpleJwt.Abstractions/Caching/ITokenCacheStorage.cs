using System;
using System.Threading;
using System.Threading.Tasks;
using SimpleJwt.Abstractions;

namespace SimpleJwt.Abstractions.Caching
{
    /// <summary>
    /// Specialized cache storage interface for JWT tokens.
    /// </summary>
    public interface ITokenCacheStorage : ICacheProvider<string, IJwtToken>
    {
        // Additional token-specific methods can be added here if needed.
    }
}
