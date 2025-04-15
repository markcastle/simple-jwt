using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using SimpleJwt.Abstractions;
using System.Threading.Tasks;

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
        private volatile bool _evictionInProgress;

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

            // Use the thread-safe AddOrUpdate method of ConcurrentDictionary
            // This ensures the token is atomically added or updated
            _tokenCache.AddOrUpdate(key, token, (_, _) => token);

            // If we've exceeded the size limit and no eviction is in progress,
            // trigger eviction on a separate thread
            if (_tokenCache.Count > _maxSize && !_evictionInProgress)
            {
                // Use a lock to ensure only one thread initiates eviction at a time
                lock (_evictionLock)
                {
                    if (_tokenCache.Count > _maxSize && !_evictionInProgress)
                    {
                        EvictOldestTokenAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Adds or updates a token in the cache and performs synchronous eviction if needed.
        /// This method is intended for testing scenarios where immediate eviction is required.
        /// </summary>
        /// <param name="key">The key to store the token under.</param>
        /// <param name="token">The token to cache.</param>
        /// <param name="evictSynchronously">Whether to evict tokens synchronously if maximum size is exceeded.</param>
        internal void AddOrUpdateTokenWithSync(string key, IJwtToken token, bool evictSynchronously)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Cache key cannot be null or empty.", nameof(key));
            }

            if (token == null)
            {
                throw new ArgumentNullException(nameof(token), "Token cannot be null.");
            }

            // Use the thread-safe AddOrUpdate method of ConcurrentDictionary
            // This ensures the token is atomically added or updated
            _tokenCache.AddOrUpdate(key, token, (_, _) => token);

            // If we've exceeded the size limit and no eviction is in progress,
            // trigger eviction on a separate thread
            if (_tokenCache.Count > _maxSize && !_evictionInProgress)
            {
                // Use a lock to ensure only one thread initiates eviction at a time
                lock (_evictionLock)
                {
                    if (_tokenCache.Count > _maxSize && !_evictionInProgress)
                    {
                        if (evictSynchronously)
                        {
                            EvictTokensSync();
                        }
                        else
                        {
                            EvictOldestTokenAsync();
                        }
                    }
                }
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
        /// Asynchronously evicts the oldest token from the cache.
        /// </summary>
        private void EvictOldestTokenAsync()
        {
            // Mark that eviction is in progress to prevent multiple concurrent evictions
            _evictionInProgress = true;
            
            // Run eviction on a separate task to avoid blocking the current thread
            Task.Run(() => 
            {
                try
                {
                    // Determine how many items to remove (20% of max size, at least 1)
                    int itemsToRemove = Math.Max(1, _maxSize / 5);
                    int removed = 0;
                    
                    // Get the keys to consider for removal
                    // Take a snapshot to avoid enumeration issues
                    List<string> keysToConsider;
                    lock (_evictionLock)
                    {
                        keysToConsider = _tokenCache.Keys.ToList();
                    }
                    
                    // Randomly select keys to remove rather than using oldest
                    // This helps prevent multiple threads from all removing the same keys
                    var random = new Random();
                    var keysToRemove = keysToConsider
                        .OrderBy(_ => random.Next())
                        .Take(itemsToRemove * 2)
                        .ToList();
                    
                    foreach (var key in keysToRemove)
                    {
                        if (_tokenCache.Count <= _maxSize || removed >= itemsToRemove)
                        {
                            break;
                        }
                        
                        if (_tokenCache.TryRemove(key, out _))
                        {
                            removed++;
                        }
                    }
                }
                finally
                {
                    // Ensure we always reset this flag
                    _evictionInProgress = false;
                }
            });
        }

        /// <summary>
        /// Synchronously evicts tokens from the cache.
        /// </summary>
        private void EvictTokensSync()
        {
            // Mark that eviction is in progress to prevent multiple concurrent evictions
            _evictionInProgress = true;
            try
            {
                // Remove items until the cache is within the size limit (strictly <= _maxSize)
                while (_tokenCache.Count > _maxSize)
                {
                    // Remove a random key (as before)
                    var keys = _tokenCache.Keys.ToList();
                    if (keys.Count == 0) break;
                    var random = new Random();
                    var keyToRemove = keys[random.Next(keys.Count)];
                    _tokenCache.TryRemove(keyToRemove, out _);
                }
            }
            finally
            {
                _evictionInProgress = false;
            }
        }
    }
} 