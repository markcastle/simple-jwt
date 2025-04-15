using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SimpleJwt.Abstractions;
using SimpleJwt.Abstractions.Caching;

namespace SimpleJwt.Core.Caching
{
    /// <summary>
    /// Default in-memory cache provider for JWT tokens and arbitrary values.
    /// </summary>
    /// <typeparam name="TKey">The type of the cache key.</typeparam>
    /// <typeparam name="TValue">The type of the cache value.</typeparam>
    public class SimpleMemoryCacheProvider<TKey, TValue> : ICacheProvider<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, (TValue Value, DateTimeOffset? Expiry)> _cache;
        private readonly Queue<TKey> _keyOrder;
        private readonly HashSet<TKey> _keySet;
        private readonly int _maxSize;
        private readonly object _evictionLock = new object();
        private volatile bool _evictionInProgress;

        /// <summary>
        /// Initializes a new instance with optional size limit.
        /// </summary>
        /// <param name="maxSize">Maximum number of items to store in cache (default: 1000).</param>
        public SimpleMemoryCacheProvider(int maxSize = 1000)
        {
            if (maxSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxSize), "Maximum cache size must be greater than zero.");
            _cache = new ConcurrentDictionary<TKey, (TValue, DateTimeOffset?)>();
            _keyOrder = new Queue<TKey>();
            _keySet = new HashSet<TKey>();
            _maxSize = maxSize;
        }

        /// <inheritdoc />
        public Task<TValue> GetAsync(TKey key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (_cache.TryGetValue(key, out var entry))
            {
                if (!entry.Expiry.HasValue || entry.Expiry > DateTimeOffset.UtcNow)
                    return Task.FromResult(entry.Value);
                RemoveAsync(key, cancellationToken).Wait(cancellationToken);
            }
            return Task.FromResult(default(TValue));
        }

        /// <inheritdoc />
        public Task SetAsync(TKey key, TValue value, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (value == null) throw new ArgumentNullException(nameof(value));
            var expiry = absoluteExpiration.HasValue ? DateTimeOffset.UtcNow.Add(absoluteExpiration.Value) : (DateTimeOffset?)null;
            _cache[key] = (value, expiry);
            lock (_evictionLock)
            {
                if (_keySet.Contains(key))
                {
                    // Remove the key from the queue to maintain true FIFO order
                    var tmpQueue = new Queue<TKey>(_keyOrder.Count);
                    while (_keyOrder.Count > 0)
                    {
                        var k = _keyOrder.Dequeue();
                        if (!EqualityComparer<TKey>.Default.Equals(k, key))
                            tmpQueue.Enqueue(k);
                    }
                    while (tmpQueue.Count > 0)
                        _keyOrder.Enqueue(tmpQueue.Dequeue());
                }
                else
                {
                    _keySet.Add(key);
                }
                _keyOrder.Enqueue(key);
            }
            TriggerEvictionIfNeeded();
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task RemoveAsync(TKey key, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (key == null) throw new ArgumentNullException(nameof(key));
            _cache.TryRemove(key, out _);
            lock (_evictionLock)
            {
                _keySet.Remove(key);
                var tmpQueue = new Queue<TKey>(_keyOrder.Count);
                while (_keyOrder.Count > 0)
                {
                    var k = _keyOrder.Dequeue();
                    if (!EqualityComparer<TKey>.Default.Equals(k, key))
                        tmpQueue.Enqueue(k);
                }
                while (tmpQueue.Count > 0)
                    _keyOrder.Enqueue(tmpQueue.Dequeue());
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task ClearAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _cache.Clear();
            lock (_evictionLock)
            {
                _keyOrder.Clear();
                _keySet.Clear();
            }
            return Task.CompletedTask;
        }

        private void TriggerEvictionIfNeeded()
        {
            if (_cache.Count > _maxSize && !_evictionInProgress)
            {
                lock (_evictionLock)
                {
                    if (_cache.Count > _maxSize && !_evictionInProgress)
                    {
                        _evictionInProgress = true;
                        // Synchronous eviction for predictability
                        EvictOldestItems();
                    }
                }
            }
        }

        private void EvictOldestItems()
        {
            try
            {
                lock (_evictionLock)
                {
                    while (_cache.Count > _maxSize && _keyOrder.Count > 0)
                    {
                        var oldestKey = _keyOrder.Dequeue();
                        if (_keySet.Remove(oldestKey))
                        {
                            _cache.TryRemove(oldestKey, out _);
                        }
                    }
                }
            }
            finally
            {
                _evictionInProgress = false;
            }
        }
    }
}
