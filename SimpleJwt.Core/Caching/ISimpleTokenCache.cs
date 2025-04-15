using System;
using SimpleJwt.Abstractions;

namespace SimpleJwt.Core.Caching
{
    /// <summary>
    /// Interface for a simple token cache that stores JWT tokens with string keys.
    /// </summary>
    public interface ISimpleTokenCache
    {
        /// <summary>
        /// Attempts to get a token from the cache.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <param name="token">The retrieved token, if found.</param>
        /// <returns>True if the token was found in the cache; otherwise, false.</returns>
        bool TryGetToken(string key, out IJwtToken token);
        
        /// <summary>
        /// Adds or updates a token in the cache.
        /// </summary>
        /// <param name="key">The key to store the token under.</param>
        /// <param name="token">The token to cache.</param>
        void AddOrUpdateToken(string key, IJwtToken token);
        
        /// <summary>
        /// Invalidates (removes) a token from the cache.
        /// </summary>
        /// <param name="key">The key of the token to invalidate.</param>
        void InvalidateToken(string key);
        
        /// <summary>
        /// Invalidates (removes) all tokens from the cache.
        /// </summary>
        void InvalidateAllTokens();
    }
} 