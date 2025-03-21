using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SimpleJwt.Abstractions;

namespace SimpleJwt.Core.TokenLifetime
{
    /// <summary>
    /// Default implementation of <see cref="ITokenRevoker"/>.
    /// </summary>
    public class JwtRevoker : ITokenRevoker
    {
        private readonly ConcurrentDictionary<string, (string Reason, DateTimeOffset ExpirationTime, string UserId)> _revokedTokens = 
            new ConcurrentDictionary<string, (string, DateTimeOffset, string)>();

        private readonly IJwtParser _parser;

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtRevoker"/> class.
        /// </summary>
        /// <param name="parser">The JWT parser.</param>
        public JwtRevoker(IJwtParser parser)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        }

        /// <summary>
        /// Asynchronously checks if a token is revoked.
        /// </summary>
        /// <param name="token">The token to check.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous check operation. The task result contains a boolean indicating whether the token is revoked.</returns>
        public Task<bool> IsRevokedAsync(string token, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(IsRevoked(token));
        }

        /// <summary>
        /// Checks if a token is revoked.
        /// </summary>
        /// <param name="token">The token to check.</param>
        /// <returns>true if the token is revoked; otherwise, false.</returns>
        public bool IsRevoked(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Token cannot be null or empty.", nameof(token));
            }

            // Check if the token is in the revoked tokens dictionary
            // and the revocation hasn't expired
            if (_revokedTokens.TryGetValue(token, out var info))
            {
                // Clean up expired revocations
                if (info.ExpirationTime < DateTimeOffset.UtcNow)
                {
                    _revokedTokens.TryRemove(token, out _);
                    return false;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Revokes a token.
        /// </summary>
        /// <param name="token">The token to revoke.</param>
        /// <param name="reason">The reason for revocation.</param>
        /// <param name="expirationTime">The time when the revocation should expire.</param>
        /// <returns>true if the token was successfully revoked; otherwise, false.</returns>
        public bool Revoke(string token, string reason = null, DateTimeOffset? expirationTime = null)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Token cannot be null or empty.", nameof(token));
            }

            try
            {
                // Parse the token to ensure it's valid
                var parsedToken = _parser.Parse(token);
                string userId = null;

                // Try to extract user ID from subject claim if available
                if (parsedToken.TryGetClaim<string>("sub", out var subject))
                {
                    userId = subject;
                }

                // Set a default expiration time if none is provided
                var expiration = expirationTime ?? DateTimeOffset.UtcNow.AddDays(7);
                
                // Add the token to the revoked tokens dictionary
                return _revokedTokens.TryAdd(token, (reason ?? "No reason provided", expiration, userId));
            }
            catch
            {
                // If the token is invalid, we can't revoke it
                return false;
            }
        }

        /// <summary>
        /// Asynchronously revokes a token.
        /// </summary>
        /// <param name="token">The token to revoke.</param>
        /// <param name="reason">The reason for revocation.</param>
        /// <param name="expirationTime">The time when the revocation should expire.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous revoke operation. The task result contains a boolean indicating whether the token was successfully revoked.</returns>
        public Task<bool> RevokeAsync(string token, string reason = null, DateTimeOffset? expirationTime = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Revoke(token, reason, expirationTime));
        }

        /// <summary>
        /// Gets the reason for a token's revocation.
        /// </summary>
        /// <param name="token">The JWT token.</param>
        /// <returns>The reason for revocation, or null if the token is not revoked.</returns>
        public string GetRevocationReason(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Token cannot be null or empty.", nameof(token));
            }

            if (_revokedTokens.TryGetValue(token, out var info))
            {
                // Clean up expired revocations
                if (info.ExpirationTime < DateTimeOffset.UtcNow)
                {
                    _revokedTokens.TryRemove(token, out _);
                    return null;
                }

                return info.Reason;
            }

            return null;
        }

        /// <summary>
        /// Asynchronously gets the reason for a token's revocation.
        /// </summary>
        /// <param name="token">The JWT token.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous get operation. The task result contains the reason for revocation, or null if the token is not revoked.</returns>
        public Task<string> GetRevocationReasonAsync(string token, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(GetRevocationReason(token));
        }

        /// <summary>
        /// Tries to get the reason a token was revoked.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="reason">When this method returns, contains the reason the token was revoked, if the token is found; otherwise, null.</param>
        /// <returns>true if the token is revoked; otherwise, false.</returns>
        public bool TryGetRevocationReason(string token, out string reason)
        {
            reason = null;

            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Token cannot be null or empty.", nameof(token));
            }

            if (_revokedTokens.TryGetValue(token, out var info))
            {
                // Clean up expired revocations
                if (info.ExpirationTime < DateTimeOffset.UtcNow)
                {
                    _revokedTokens.TryRemove(token, out _);
                    return false;
                }

                reason = info.Reason;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Asynchronously tries to get the reason a token was revoked.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="reason">When this method returns, contains the reason the token was revoked, if the token is found; otherwise, null.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the token is revoked.</returns>
        public Task<bool> TryGetRevocationReasonAsync(string token, out string reason, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(TryGetRevocationReason(token, out reason));
        }

        /// <summary>
        /// Asynchronously tries to get the reason a token was revoked.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a tuple with a boolean indicating whether the token is revoked and the reason if it is revoked.</returns>
        public Task<(bool success, string reason)> TryGetRevocationReasonAsync(string token, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var success = TryGetRevocationReason(token, out var reason);
            return Task.FromResult((success, reason));
        }

        /// <summary>
        /// Revokes all tokens for a user.
        /// </summary>
        /// <param name="userId">The ID of the user whose tokens to revoke.</param>
        /// <param name="reason">The reason for revocation.</param>
        /// <returns>The number of tokens revoked.</returns>
        public int RevokeAllForUser(string userId, string reason = null)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            }

            int count = 0;
            var tokens = GetTokensForUser(userId);

            foreach (var token in tokens)
            {
                if (Revoke(token, reason ?? $"All tokens revoked for user {userId}"))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Asynchronously revokes all tokens for a user.
        /// </summary>
        /// <param name="userId">The ID of the user whose tokens to revoke.</param>
        /// <param name="reason">The reason for revocation.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous revocation operation. The task result contains the number of tokens revoked.</returns>
        public Task<int> RevokeAllForUserAsync(string userId, string reason = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(RevokeAllForUser(userId, reason));
        }

        private IEnumerable<string> GetTokensForUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            }

            return _revokedTokens
                .Where(kvp => kvp.Value.UserId == userId)
                .Select(kvp => kvp.Key)
                .ToList();
        }
    }
} 