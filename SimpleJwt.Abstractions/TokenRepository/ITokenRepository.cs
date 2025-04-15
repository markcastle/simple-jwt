using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleJwt.Abstractions.TokenRepository
{
    /// <summary>
    /// Defines a repository for storing and retrieving JWT tokens.
    /// </summary>
    public interface ITokenRepository
    {
        /// <summary>
        /// Stores a token in the repository.
        /// </summary>
        /// <param name="token">The JWT token.</param>
        /// <param name="userId">The ID of the user the token belongs to.</param>
        /// <param name="expirationTime">The expiration time of the token.</param>
        /// <param name="tokenType">The type of token (e.g., "access", "refresh").</param>
        /// <param name="metadata">Optional metadata associated with the token.</param>
        /// <returns>True if the token was successfully stored; otherwise, false.</returns>
        bool StoreToken(string token, string userId, DateTimeOffset expirationTime, string tokenType = "access", IDictionary<string, object> metadata = null);

        /// <summary>
        /// Asynchronously stores a token in the repository.
        /// </summary>
        /// <param name="token">The JWT token.</param>
        /// <param name="userId">The ID of the user the token belongs to.</param>
        /// <param name="expirationTime">The expiration time of the token.</param>
        /// <param name="tokenType">The type of token (e.g., "access", "refresh").</param>
        /// <param name="metadata">Optional metadata associated with the token.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the token was successfully stored.</returns>
        Task<bool> StoreTokenAsync(string token, string userId, DateTimeOffset expirationTime, string tokenType = "access", IDictionary<string, object> metadata = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a token from the repository.
        /// </summary>
        /// <param name="token">The JWT token.</param>
        /// <returns>The stored token information, or null if the token is not found.</returns>
        TokenInfo GetToken(string token);

        /// <summary>
        /// Asynchronously gets a token from the repository.
        /// </summary>
        /// <param name="token">The JWT token.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the stored token information, or null if the token is not found.</returns>
        Task<TokenInfo> GetTokenAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all tokens for a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="tokenType">Optional. The type of token to get.</param>
        /// <returns>A collection of token information for the user.</returns>
        IEnumerable<TokenInfo> GetTokensForUser(string userId, string tokenType = null);

        /// <summary>
        /// Asynchronously gets all tokens for a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="tokenType">Optional. The type of token to get.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of token information for the user.</returns>
        Task<IEnumerable<TokenInfo>> GetTokensForUserAsync(string userId, string tokenType = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes a token from the repository.
        /// </summary>
        /// <param name="token">The JWT token.</param>
        /// <returns>True if the token was successfully removed; otherwise, false.</returns>
        bool RemoveToken(string token);

        /// <summary>
        /// Asynchronously removes a token from the repository.
        /// </summary>
        /// <param name="token">The JWT token.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the token was successfully removed.</returns>
        Task<bool> RemoveTokenAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes all tokens for a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="tokenType">Optional. The type of token to remove.</param>
        /// <returns>The number of tokens removed.</returns>
        int RemoveTokensForUser(string userId, string tokenType = null);

        /// <summary>
        /// Asynchronously removes all tokens for a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="tokenType">Optional. The type of token to remove.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of tokens removed.</returns>
        Task<int> RemoveTokensForUserAsync(string userId, string tokenType = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes all expired tokens from the repository.
        /// </summary>
        /// <param name="beforeTime">Optional. Remove tokens that expired before this time. Default is the current time.</param>
        /// <returns>The number of tokens removed.</returns>
        int RemoveExpiredTokens(DateTimeOffset? beforeTime = null);

        /// <summary>
        /// Asynchronously removes all expired tokens from the repository.
        /// </summary>
        /// <param name="beforeTime">Optional. Remove tokens that expired before this time. Default is the current time.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of tokens removed.</returns>
        Task<int> RemoveExpiredTokensAsync(DateTimeOffset? beforeTime = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a token exists in the repository.
        /// </summary>
        /// <param name="token">The JWT token.</param>
        /// <returns>True if the token exists; otherwise, false.</returns>
        bool TokenExists(string token);

        /// <summary>
        /// Asynchronously checks if a token exists in the repository.
        /// </summary>
        /// <param name="token">The JWT token.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the token exists.</returns>
        Task<bool> TokenExistsAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the number of tokens in the repository.
        /// </summary>
        /// <param name="userId">Optional. The ID of the user whose tokens to count.</param>
        /// <param name="tokenType">Optional. The type of token to count.</param>
        /// <returns>The number of tokens.</returns>
        int GetTokenCount(string userId = null, string tokenType = null);

        /// <summary>
        /// Asynchronously gets the number of tokens in the repository.
        /// </summary>
        /// <param name="userId">Optional. The ID of the user whose tokens to count.</param>
        /// <param name="tokenType">Optional. The type of token to count.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of tokens.</returns>
        Task<int> GetTokenCountAsync(string userId = null, string tokenType = null, CancellationToken cancellationToken = default);
    }
} 