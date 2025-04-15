using System;
using System.Collections.Generic;

namespace SimpleJwt.Abstractions.TokenRepository
{
    /// <summary>
    /// Represents information about a stored JWT token.
    /// </summary>
    public class TokenInfo
    {
        /// <summary>
        /// Gets or sets the token string.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user the token belongs to.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Gets or sets the time when the token was created.
        /// </summary>
        public DateTimeOffset CreationTime { get; set; }

        /// <summary>
        /// Gets or sets the expiration time of the token.
        /// </summary>
        public DateTimeOffset ExpirationTime { get; set; }

        /// <summary>
        /// Gets or sets the type of the token (e.g., "access", "refresh").
        /// </summary>
        public string TokenType { get; set; }

        /// <summary>
        /// Gets or sets the metadata associated with the token.
        /// </summary>
        public IDictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// Gets a value indicating whether the token has expired.
        /// </summary>
        public bool IsExpired => DateTimeOffset.UtcNow > ExpirationTime;

        /// <summary>
        /// Gets the remaining time until the token expires.
        /// </summary>
        public TimeSpan TimeUntilExpiration => IsExpired ? TimeSpan.Zero : ExpirationTime - DateTimeOffset.UtcNow;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenInfo"/> class.
        /// </summary>
        public TokenInfo()
        {
            CreationTime = DateTimeOffset.UtcNow;
            Metadata = new Dictionary<string, object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenInfo"/> class.
        /// </summary>
        /// <param name="token">The token string.</param>
        /// <param name="userId">The ID of the user the token belongs to.</param>
        /// <param name="expirationTime">The expiration time of the token.</param>
        /// <param name="tokenType">The type of token.</param>
        /// <param name="metadata">Optional metadata associated with the token.</param>
        public TokenInfo(string token, string userId, DateTimeOffset expirationTime, string tokenType = "access", IDictionary<string, object> metadata = null)
        {
            Token = token ?? throw new ArgumentNullException(nameof(token));
            UserId = userId ?? throw new ArgumentNullException(nameof(userId));
            ExpirationTime = expirationTime;
            TokenType = tokenType ?? "access";
            CreationTime = DateTimeOffset.UtcNow;
            Metadata = metadata ?? new Dictionary<string, object>();
        }
    }
} 