using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SimpleJwt.Abstractions;
using SimpleJwt.Abstractions.TokenLifetime;
using SimpleJwt.Abstractions.TokenRepository;

namespace SimpleJwt.Core.TokenLifetime
{
    /// <summary>
    /// Implementation of <see cref="ITokenRevoker"/> that uses a token repository to track revoked tokens.
    /// </summary>
    public class TokenRepositoryRevoker : ITokenRevoker
    {
        private readonly ITokenRepository _repository;
        private readonly IJwtParser _parser;
        private const string RevokedTokenType = "revoked";
        private const string RevocationReasonKey = "revocationReason";

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenRepositoryRevoker"/> class.
        /// </summary>
        /// <param name="repository">The token repository.</param>
        /// <param name="parser">The JWT parser.</param>
        public TokenRepositoryRevoker(ITokenRepository repository, IJwtParser parser)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        }

        /// <inheritdoc />
        public bool IsRevoked(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Token cannot be null or empty.", nameof(token));
            }

            return _repository.TokenExists(token);
        }

        /// <inheritdoc />
        public Task<bool> IsRevokedAsync(string token, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Token cannot be null or empty.", nameof(token));
            }

            return _repository.TokenExistsAsync(token, cancellationToken);
        }

        /// <inheritdoc />
        public bool Revoke(string token, string reason = null, DateTimeOffset? expirationTime = null)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Token cannot be null or empty.", nameof(token));
            }

            try
            {
                // Parse the token to ensure it's valid and get the expiration time and user ID
                var parsedToken = _parser.Parse(token);
                string userId = null;
                DateTimeOffset tokenExpiration = DateTimeOffset.UtcNow.AddDays(7); // Default if not in token

                // Try to extract user ID from subject claim if available
                if (parsedToken.TryGetClaim<string>("sub", out var subject))
                {
                    userId = subject;
                }

                // Try to extract expiration time if available
                if (parsedToken.TryGetClaim<long>("exp", out var exp))
                {
                    tokenExpiration = DateTimeOffset.FromUnixTimeSeconds(exp);
                }

                // Use the provided expiration time or the token's expiration time
                var revocationExpiration = expirationTime ?? tokenExpiration;

                // Add revocation metadata
                var metadata = new Dictionary<string, object>
                {
                    { RevocationReasonKey, reason ?? "No reason provided" }
                };

                // Store the revoked token in the repository
                return _repository.StoreToken(token, userId ?? "unknown", revocationExpiration, RevokedTokenType, metadata);
            }
            catch
            {
                // If the token is invalid, we can't revoke it
                return false;
            }
        }

        /// <inheritdoc />
        public Task<bool> RevokeAsync(string token, string reason = null, DateTimeOffset? expirationTime = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Token cannot be null or empty.", nameof(token));
            }

            try
            {
                // Parse the token to ensure it's valid and get the expiration time and user ID
                var parsedToken = _parser.Parse(token);
                string userId = null;
                DateTimeOffset tokenExpiration = DateTimeOffset.UtcNow.AddDays(7); // Default if not in token

                // Try to extract user ID from subject claim if available
                if (parsedToken.TryGetClaim<string>("sub", out var subject))
                {
                    userId = subject;
                }

                // Try to extract expiration time if available
                if (parsedToken.TryGetClaim<long>("exp", out var exp))
                {
                    tokenExpiration = DateTimeOffset.FromUnixTimeSeconds(exp);
                }

                // Use the provided expiration time or the token's expiration time
                var revocationExpiration = expirationTime ?? tokenExpiration;

                // Add revocation metadata
                var metadata = new Dictionary<string, object>
                {
                    { RevocationReasonKey, reason ?? "No reason provided" }
                };

                // Store the revoked token in the repository
                return _repository.StoreTokenAsync(token, userId ?? "unknown", revocationExpiration, RevokedTokenType, metadata, cancellationToken);
            }
            catch
            {
                // If the token is invalid, we can't revoke it
                return Task.FromResult(false);
            }
        }

        /// <inheritdoc />
        public string GetRevocationReason(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Token cannot be null or empty.", nameof(token));
            }

            var tokenInfo = _repository.GetToken(token);
            if (tokenInfo != null && tokenInfo.Metadata.TryGetValue(RevocationReasonKey, out var reasonObj))
            {
                return reasonObj.ToString();
            }

            return null;
        }

        /// <inheritdoc />
        public async Task<string> GetRevocationReasonAsync(string token, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Token cannot be null or empty.", nameof(token));
            }

            var tokenInfo = await _repository.GetTokenAsync(token, cancellationToken).ConfigureAwait(false);
            if (tokenInfo != null && tokenInfo.Metadata.TryGetValue(RevocationReasonKey, out var reasonObj))
            {
                return reasonObj.ToString();
            }

            return null;
        }

        /// <inheritdoc />
        public int RevokeAllForUser(string userId, string reason = null)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            }

            var tokens = _repository.GetTokensForUser(userId);
            int count = 0;

            // Revoke each token
            foreach (var tokenInfo in tokens)
            {
                if (Revoke(tokenInfo.Token, reason ?? $"All tokens revoked for user {userId}"))
                {
                    count++;
                }
            }

            return count;
        }

        /// <inheritdoc />
        public async Task<int> RevokeAllForUserAsync(string userId, string reason = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            }

            var tokens = await _repository.GetTokensForUserAsync(userId, cancellationToken: cancellationToken).ConfigureAwait(false);
            int count = 0;

            // Revoke each token
            foreach (var tokenInfo in tokens)
            {
                if (await RevokeAsync(tokenInfo.Token, reason ?? $"All tokens revoked for user {userId}", cancellationToken: cancellationToken).ConfigureAwait(false))
                {
                    count++;
                }
            }

            return count;
        }

        /// <inheritdoc />
        public int RevokeTokens(IEnumerable<string> tokens, string reason = null)
        {
            if (tokens == null)
            {
                throw new ArgumentNullException(nameof(tokens));
            }

            int count = 0;
            foreach (var token in tokens)
            {
                if (Revoke(token, reason))
                {
                    count++;
                }
            }

            return count;
        }

        /// <inheritdoc />
        public async Task<int> RevokeTokensAsync(IEnumerable<string> tokens, string reason = null, CancellationToken cancellationToken = default)
        {
            if (tokens == null)
            {
                throw new ArgumentNullException(nameof(tokens));
            }

            int count = 0;
            foreach (var token in tokens)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (await RevokeAsync(token, reason, cancellationToken: cancellationToken).ConfigureAwait(false))
                {
                    count++;
                }
            }

            return count;
        }
    }
} 