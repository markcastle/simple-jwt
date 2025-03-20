using System;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleJwt.Core.TokenLifetime
{
    /// <summary>
    /// Provides functionality to refresh JWT tokens.
    /// </summary>
    public interface ITokenRefresher
    {
        /// <summary>
        /// Refreshes a JWT token using its corresponding refresh token.
        /// </summary>
        /// <param name="accessToken">The JWT access token to refresh.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <returns>A <see cref="RefreshResult"/> containing the new access token and refresh token.</returns>
        RefreshResult Refresh(string accessToken, string refreshToken);

        /// <summary>
        /// Asynchronously refreshes a JWT token using its corresponding refresh token.
        /// </summary>
        /// <param name="accessToken">The JWT access token to refresh.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous refresh operation. The task result contains a <see cref="RefreshResult"/> with the new access token and refresh token.</returns>
        Task<RefreshResult> RefreshAsync(string accessToken, string refreshToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a refresh token for a JWT access token.
        /// </summary>
        /// <param name="accessToken">The JWT access token to create a refresh token for.</param>
        /// <param name="lifetime">The lifetime of the refresh token.</param>
        /// <returns>A refresh token.</returns>
        string CreateRefreshToken(string accessToken, TimeSpan lifetime);

        /// <summary>
        /// Asynchronously creates a refresh token for a JWT access token.
        /// </summary>
        /// <param name="accessToken">The JWT access token to create a refresh token for.</param>
        /// <param name="lifetime">The lifetime of the refresh token.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous create operation. The task result contains the refresh token.</returns>
        Task<string> CreateRefreshTokenAsync(string accessToken, TimeSpan lifetime, CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates a refresh token for a JWT access token.
        /// </summary>
        /// <param name="accessToken">The JWT access token.</param>
        /// <param name="refreshToken">The refresh token to validate.</param>
        /// <returns>true if the refresh token is valid for the access token; otherwise, false.</returns>
        bool ValidateRefreshToken(string accessToken, string refreshToken);

        /// <summary>
        /// Asynchronously validates a refresh token for a JWT access token.
        /// </summary>
        /// <param name="accessToken">The JWT access token.</param>
        /// <param name="refreshToken">The refresh token to validate.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous validation operation. The task result contains a boolean indicating whether the refresh token is valid for the access token.</returns>
        Task<bool> ValidateRefreshTokenAsync(string accessToken, string refreshToken, CancellationToken cancellationToken = default);
    }
} 