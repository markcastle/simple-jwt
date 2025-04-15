using System;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleJwt.Abstractions.Caching
{
    /// <summary>
    /// Generic cache provider abstraction for flexible caching backends.
    /// </summary>
    /// <typeparam name="TKey">The type of the cache key.</typeparam>
    /// <typeparam name="TValue">The type of the cache value.</typeparam>
    public interface ICacheProvider<TKey, TValue>
    {
        /// <summary>
        /// Gets a value from the cache asynchronously.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>The cached value, or default if not found.</returns>
        Task<TValue> GetAsync(TKey key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets a value in the cache asynchronously.
        /// </summary>
        /// <param name="key">The key to store the value under.</param>
        /// <param name="value">The value to cache.</param>
        /// <param name="absoluteExpiration">Optional absolute expiration time.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SetAsync(TKey key, TValue value, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a value from the cache asynchronously.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RemoveAsync(TKey key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Clears all values from the cache asynchronously.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ClearAsync(CancellationToken cancellationToken = default);
    }
}
