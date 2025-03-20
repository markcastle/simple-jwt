using System;

namespace SimpleJwt.Core.TokenLifetime
{
    /// <summary>
    /// Represents the result of a JWT token refresh operation.
    /// </summary>
    public class RefreshResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshResult"/> class for a successful refresh.
        /// </summary>
        /// <param name="accessToken">The new JWT access token.</param>
        /// <param name="refreshToken">The new refresh token.</param>
        /// <param name="accessTokenExpiresAt">The expiration time of the access token.</param>
        /// <param name="refreshTokenExpiresAt">The expiration time of the refresh token.</param>
        public RefreshResult(string accessToken, string refreshToken, DateTimeOffset accessTokenExpiresAt, DateTimeOffset refreshTokenExpiresAt)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new ArgumentException("Access token cannot be null or empty.", nameof(accessToken));
            }

            if (string.IsNullOrEmpty(refreshToken))
            {
                throw new ArgumentException("Refresh token cannot be null or empty.", nameof(refreshToken));
            }

            IsSuccess = true;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            AccessTokenExpiresAt = accessTokenExpiresAt;
            RefreshTokenExpiresAt = refreshTokenExpiresAt;
            Error = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshResult"/> class for a failed refresh.
        /// </summary>
        /// <param name="error">The error that occurred during refresh.</param>
        public RefreshResult(string error)
        {
            if (string.IsNullOrEmpty(error))
            {
                throw new ArgumentException("Error cannot be null or empty.", nameof(error));
            }

            IsSuccess = false;
            AccessToken = null;
            RefreshToken = null;
            AccessTokenExpiresAt = DateTimeOffset.MinValue;
            RefreshTokenExpiresAt = DateTimeOffset.MinValue;
            Error = error;
        }

        /// <summary>
        /// Gets a value indicating whether the refresh operation was successful.
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// Gets the new JWT access token, or null if the refresh failed.
        /// </summary>
        public string AccessToken { get; }

        /// <summary>
        /// Gets the new refresh token, or null if the refresh failed.
        /// </summary>
        public string RefreshToken { get; }

        /// <summary>
        /// Gets the expiration time of the access token, or DateTimeOffset.MinValue if the refresh failed.
        /// </summary>
        public DateTimeOffset AccessTokenExpiresAt { get; }

        /// <summary>
        /// Gets the expiration time of the refresh token, or DateTimeOffset.MinValue if the refresh failed.
        /// </summary>
        public DateTimeOffset RefreshTokenExpiresAt { get; }

        /// <summary>
        /// Gets the error that occurred during refresh, or null if the refresh was successful.
        /// </summary>
        public string Error { get; }
    }
} 