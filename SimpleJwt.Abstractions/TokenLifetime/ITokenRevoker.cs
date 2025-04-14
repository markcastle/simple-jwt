using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SimpleJwt.Abstractions.TokenLifetime
{
    /// <summary>
    /// Provides functionality to revoke JWT tokens.
    /// </summary>
    public interface ITokenRevoker
    {
        /// <summary>
        /// Revokes a JWT token.
        /// </summary>
        /// <param name="token">The JWT token to revoke.</param>
        /// <param name="reason">The reason for revocation.</param>
        /// <param name="expirationTime">Optional. The time until which the token should be considered revoked. If null, the token is revoked permanently.</param>
        /// <returns>true if the token was successfully revoked; otherwise, false.</returns>
        bool Revoke(string token, string reason = null, DateTimeOffset? expirationTime = null);

        /// <summary>
        /// Asynchronously revokes a JWT token.
        /// </summary>
        /// <param name="token">The JWT token to revoke.</param>
        /// <param name="reason">The reason for revocation.</param>
        /// <param name="expirationTime">Optional. The time until which the token should be considered revoked. If null, the token is revoked permanently.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous revocation operation. The task result contains a boolean indicating whether the token was successfully revoked.</returns>
        Task<bool> RevokeAsync(string token, string reason = null, DateTimeOffset? expirationTime = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a JWT token is revoked.
        /// </summary>
        /// <param name="token">The JWT token to check.</param>
        /// <returns>true if the token is revoked; otherwise, false.</returns>
        bool IsRevoked(string token);

        /// <summary>
        /// Asynchronously checks if a JWT token is revoked.
        /// </summary>
        /// <param name="token">The JWT token to check.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous check operation. The task result contains a boolean indicating whether the token is revoked.</returns>
        Task<bool> IsRevokedAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the reason for a token's revocation.
        /// </summary>
        /// <param name="token">The JWT token.</param>
        /// <returns>The reason for revocation, or null if the token is not revoked.</returns>
        string GetRevocationReason(string token);

        /// <summary>
        /// Asynchronously gets the reason for a token's revocation.
        /// </summary>
        /// <param name="token">The JWT token.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous get operation. The task result contains the reason for revocation, or null if the token is not revoked.</returns>
        Task<string> GetRevocationReasonAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Revokes all tokens for a user.
        /// </summary>
        /// <param name="userId">The ID of the user whose tokens to revoke.</param>
        /// <param name="reason">The reason for revocation.</param>
        /// <returns>The number of tokens revoked.</returns>
        int RevokeAllForUser(string userId, string reason = null);

        /// <summary>
        /// Asynchronously revokes all tokens for a user.
        /// </summary>
        /// <param name="userId">The ID of the user whose tokens to revoke.</param>
        /// <param name="reason">The reason for revocation.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous revocation operation. The task result contains the number of tokens revoked.</returns>
        Task<int> RevokeAllForUserAsync(string userId, string reason = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Revokes multiple tokens at once.
        /// </summary>
        /// <param name="tokens">The tokens to revoke.</param>
        /// <param name="reason">The reason for revocation.</param>
        /// <returns>The number of tokens successfully revoked.</returns>
        int RevokeTokens(IEnumerable<string> tokens, string reason = null);

        /// <summary>
        /// Asynchronously revokes multiple tokens at once.
        /// </summary>
        /// <param name="tokens">The tokens to revoke.</param>
        /// <param name="reason">The reason for revocation.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous revocation operation. The task result contains the number of tokens successfully revoked.</returns>
        Task<int> RevokeTokensAsync(IEnumerable<string> tokens, string reason = null, CancellationToken cancellationToken = default);
    }
} 