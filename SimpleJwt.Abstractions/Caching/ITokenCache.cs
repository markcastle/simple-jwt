using System;
using System.Threading;
using System.Threading.Tasks;
using SimpleJwt.Abstractions.Validation;

namespace SimpleJwt.Abstractions.Caching
{
    /// <summary>
    /// Provides caching capabilities for JWT tokens and their validation results.
    /// </summary>
    public interface ITokenCache
    {
        /// <summary>
        /// Gets a cached validation result for the specified token.
        /// </summary>
        /// <param name="token">The JWT token.</param>
        /// <returns>The cached validation result, or null if not found in the cache.</returns>
        ValidationResult GetValidationResult(string token);

        /// <summary>
        /// Asynchronously gets a cached validation result for the specified token.
        /// </summary>
        /// <param name="token">The JWT token.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous get operation. The task result contains the cached validation result, or null if not found in the cache.</returns>
        Task<ValidationResult> GetValidationResultAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a validation result to the cache for the specified token.
        /// </summary>
        /// <param name="token">The JWT token.</param>
        /// <param name="validationResult">The validation result to cache.</param>
        /// <param name="absoluteExpiration">The absolute expiration time for the cache entry.</param>
        void SetValidationResult(string token, ValidationResult validationResult, DateTimeOffset absoluteExpiration);

        /// <summary>
        /// Asynchronously adds a validation result to the cache for the specified token.
        /// </summary>
        /// <param name="token">The JWT token.</param>
        /// <param name="validationResult">The validation result to cache.</param>
        /// <param name="absoluteExpiration">The absolute expiration time for the cache entry.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous set operation.</returns>
        Task SetValidationResultAsync(string token, ValidationResult validationResult, DateTimeOffset absoluteExpiration, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a validation result from the cache for the specified token.
        /// </summary>
        /// <param name="token">The JWT token.</param>
        void RemoveValidationResult(string token);

        /// <summary>
        /// Asynchronously removes a validation result from the cache for the specified token.
        /// </summary>
        /// <param name="token">The JWT token.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous remove operation.</returns>
        Task RemoveValidationResultAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a cached parsed token for the specified token string.
        /// </summary>
        /// <param name="token">The JWT token string.</param>
        /// <returns>The cached parsed token, or null if not found in the cache.</returns>
        IJwtToken GetParsedToken(string token);

        /// <summary>
        /// Asynchronously gets a cached parsed token for the specified token string.
        /// </summary>
        /// <param name="token">The JWT token string.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous get operation. The task result contains the cached parsed token, or null if not found in the cache.</returns>
        Task<IJwtToken> GetParsedTokenAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Adds a parsed token to the cache for the specified token string.
        /// </summary>
        /// <param name="token">The JWT token string.</param>
        /// <param name="parsedToken">The parsed token to cache.</param>
        /// <param name="absoluteExpiration">The absolute expiration time for the cache entry.</param>
        void SetParsedToken(string token, IJwtToken parsedToken, DateTimeOffset absoluteExpiration);

        /// <summary>
        /// Asynchronously adds a parsed token to the cache for the specified token string.
        /// </summary>
        /// <param name="token">The JWT token string.</param>
        /// <param name="parsedToken">The parsed token to cache.</param>
        /// <param name="absoluteExpiration">The absolute expiration time for the cache entry.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous set operation.</returns>
        Task SetParsedTokenAsync(string token, IJwtToken parsedToken, DateTimeOffset absoluteExpiration, CancellationToken cancellationToken = default);
    }
} 