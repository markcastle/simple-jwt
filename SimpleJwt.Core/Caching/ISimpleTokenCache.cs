using System;
using SimpleJwt.Abstractions;

namespace SimpleJwt.Core.Caching
{
    /// <summary>
    /// Defines a simple cache interface for JWT tokens.
    /// </summary>
    public interface ISimpleTokenCache
    {
        /// <summary>
        /// Attempts to retrieve a token from the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="token">The retrieved token, if found; otherwise, null.</param>
        /// <returns><c>true</c> if the token was found in the cache; otherwise, <c>false</c>.</returns>
        bool TryGetToken(string key, out IJwtToken token);

        /// <summary>
        /// Adds or updates a token in the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="token">The token to cache.</param>
        void AddOrUpdateToken(string key, IJwtToken token);

        /// <summary>
        /// Invalidates (removes) a token from the cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        void InvalidateToken(string key);

        /// <summary>
        /// Invalidates (removes) all tokens from the cache.
        /// </summary>
        void InvalidateAllTokens();
    }
} 