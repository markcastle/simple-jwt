using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SimpleJwt.Abstractions;

namespace SimpleJwt.Core.Caching
{
    /// <summary>
    /// An in-memory implementation of the <see cref="ISimpleTokenCache"/> interface.
    /// </summary>
    public class InMemoryTokenCache : ISimpleTokenCache
    {
        private readonly ConcurrentDictionary<string, IJwtToken> _tokenCache;
        private readonly int _maxSize;
        private readonly object _evictionLock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryTokenCache"/> class with default settings.
        /// </summary>
        public InMemoryTokenCache() : this(1000)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryTokenCache"/> class with a specified maximum size.
        /// </summary>
        /// <param name="maxSize">The maximum number of tokens to store in the cache.</param>
        public InMemoryTokenCache(int maxSize)
        {
            if (maxSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxSize), "Maximum cache size must be greater than zero.");
            }

            _tokenCache = new ConcurrentDictionary<string, IJwtToken>();
            _maxSize = maxSize;
        }

        /// <inheritdoc />
        public bool TryGetToken(string key, out IJwtToken token)
        {
            if (string.IsNullOrEmpty(key))
            {
                token = null;
                return false;
            }

            return _tokenCache.TryGetValue(key, out token);
        }

        /// <inheritdoc />
        public void AddOrUpdateToken(string key, IJwtToken token)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Cache key cannot be null or empty.", nameof(key));
            }

            if (token == null)
            {
                throw new ArgumentNullException(nameof(token), "Token cannot be null.");
            }

            // Use a lock to ensure the check and add/update operations happen atomically
            lock (_evictionLock)
            {
                // Check if we need to evict tokens due to size limits
                // Do this before adding the new token to ensure we don't exceed the limit
                if (_tokenCache.Count >= _maxSize && !_tokenCache.ContainsKey(key))
                {
                    EvictOldestToken();
                }

                // Add or update the token in the cache
                _tokenCache[key] = token;
            }
        }

        /// <inheritdoc />
        public void InvalidateToken(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return; // Nothing to invalidate
            }

            _tokenCache.TryRemove(key, out _);
        }

        /// <inheritdoc />
        public void InvalidateAllTokens()
        {
            _tokenCache.Clear();
        }

        /// <summary>
        /// Evicts the oldest token from the cache.
        /// This method should be called with the _evictionLock already acquired.
        /// </summary>
        private void EvictOldestToken()
        {
            // Since we're under lock, it's safe to access the first key
            // Note that this is not truly LRU, but it's a simple approximation
            // For a true LRU implementation, we would need to track access times
            if (_tokenCache.Count > 0)
            {
                var keyToRemove = _tokenCache.Keys.FirstOrDefault();
                if (keyToRemove != null)
                {
                    _tokenCache.TryRemove(keyToRemove, out _);
                }
            }
        }
    }
} 