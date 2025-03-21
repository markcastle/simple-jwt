using System;
using System.Threading;
using System.Threading.Tasks;
using SimpleJwt.Abstractions;
using SimpleJwt.Abstractions.Validation;

namespace SimpleJwt.Core.TokenLifetime
{
    /// <summary>
    /// Default implementation of <see cref="ITokenRefresher"/>.
    /// </summary>
    public class JwtRefresher : ITokenRefresher
    {
        private readonly IJwtValidator _validator;
        private readonly IJwtParser _parser;

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtRefresher"/> class.
        /// </summary>
        /// <param name="validator">The JWT validator.</param>
        /// <param name="parser">The JWT parser.</param>
        public JwtRefresher(IJwtValidator validator, IJwtParser parser)
        {
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        }

        /// <summary>
        /// Creates a refresh token for a JWT access token.
        /// </summary>
        /// <param name="accessToken">The JWT access token to create a refresh token for.</param>
        /// <param name="lifetime">The lifetime of the refresh token.</param>
        /// <returns>A refresh token.</returns>
        public string CreateRefreshToken(string accessToken, TimeSpan lifetime)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new ArgumentException("Access token cannot be null or empty.", nameof(accessToken));
            }

            // Simple placeholder implementation - in a real implementation, this would be more secure
            // For now, we just append a signature to the token
            return $"{accessToken}.refresh.{DateTime.UtcNow.AddTicks(lifetime.Ticks).Ticks}";
        }

        /// <summary>
        /// Asynchronously creates a refresh token for a JWT access token.
        /// </summary>
        /// <param name="accessToken">The JWT access token to create a refresh token for.</param>
        /// <param name="lifetime">The lifetime of the refresh token.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous create operation. The task result contains the refresh token.</returns>
        public Task<string> CreateRefreshTokenAsync(string accessToken, TimeSpan lifetime, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(CreateRefreshToken(accessToken, lifetime));
        }

        /// <summary>
        /// Refreshes a JWT token using its corresponding refresh token.
        /// </summary>
        /// <param name="accessToken">The JWT access token to refresh.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <returns>A <see cref="RefreshResult"/> containing the new access token and refresh token.</returns>
        public RefreshResult Refresh(string accessToken, string refreshToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new ArgumentException("Access token cannot be null or empty.", nameof(accessToken));
            }

            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new ArgumentException("Refresh token cannot be null or empty.", nameof(refreshToken));
            }

            if (!ValidateRefreshToken(accessToken, refreshToken))
            {
                return new RefreshResult("Invalid refresh token.");
            }

            try
            {
                // Parse the original token
                var token = _parser.Parse(accessToken);
                
                // This is a very simplistic implementation - in a real scenario,
                // you would use the IJwtBuilder to create a new token with updated claims
                // and possibly rotate keys
                
                // For now, we'll just simulate a new token by adding an extension
                string newAccessToken = $"{accessToken}.refreshed";
                string newRefreshToken = CreateRefreshToken(newAccessToken, TimeSpan.FromDays(30));
                
                return new RefreshResult(
                    newAccessToken,
                    newRefreshToken,
                    DateTimeOffset.UtcNow.AddHours(1),
                    DateTimeOffset.UtcNow.AddDays(30));
            }
            catch (Exception ex)
            {
                return new RefreshResult($"Failed to refresh token: {ex.Message}");
            }
        }

        /// <summary>
        /// Asynchronously refreshes a JWT token using its corresponding refresh token.
        /// </summary>
        /// <param name="accessToken">The JWT access token to refresh.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous refresh operation. The task result contains a <see cref="RefreshResult"/> with the new access token and refresh token.</returns>
        public Task<RefreshResult> RefreshAsync(string accessToken, string refreshToken, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Refresh(accessToken, refreshToken));
        }

        /// <summary>
        /// Validates a refresh token for a JWT access token.
        /// </summary>
        /// <param name="accessToken">The JWT access token.</param>
        /// <param name="refreshToken">The refresh token to validate.</param>
        /// <returns>true if the refresh token is valid for the access token; otherwise, false.</returns>
        public bool ValidateRefreshToken(string accessToken, string refreshToken)
        {
            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
            {
                return false;
            }

            // Simple validation logic - in a real implementation, this would be more secure
            // We just check if the token starts with the access token and contains a timestamp
            if (!refreshToken.StartsWith($"{accessToken}.refresh."))
            {
                return false;
            }

            try
            {
                // Extract expiration timestamp
                string[] parts = refreshToken.Split('.');
                if (parts.Length < 3)
                {
                    return false;
                }

                if (!long.TryParse(parts[parts.Length - 1], out long expirationTicks))
                {
                    return false;
                }

                // Check if expired
                return expirationTicks > DateTime.UtcNow.Ticks;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Asynchronously validates a refresh token for a JWT access token.
        /// </summary>
        /// <param name="accessToken">The JWT access token.</param>
        /// <param name="refreshToken">The refresh token to validate.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous validation operation. The task result contains a boolean indicating whether the refresh token is valid for the access token.</returns>
        public Task<bool> ValidateRefreshTokenAsync(string accessToken, string refreshToken, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(ValidateRefreshToken(accessToken, refreshToken));
        }
    }
} 