using System;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleJwt.Abstractions
{
    /// <summary>
    /// Provides functionality to parse JWT tokens.
    /// </summary>
    public interface IJwtParser
    {
        /// <summary>
        /// Parses a JWT token string into an <see cref="IJwtToken"/> instance.
        /// </summary>
        /// <param name="token">The JWT token string to parse.</param>
        /// <returns>An <see cref="IJwtToken"/> instance representing the parsed token.</returns>
        /// <exception cref="ArgumentException">Thrown when the token is null, empty, or not a valid JWT format.</exception>
        IJwtToken Parse(string token);

        /// <summary>
        /// Attempts to parse a JWT token string into an <see cref="IJwtToken"/> instance.
        /// </summary>
        /// <param name="token">The JWT token string to parse.</param>
        /// <param name="result">When this method returns, contains the parsed token if successful; otherwise, null.</param>
        /// <returns>true if the token was successfully parsed; otherwise, false.</returns>
        bool TryParse(string token, out IJwtToken result);
        
        /// <summary>
        /// Asynchronously parses a JWT token string into an <see cref="IJwtToken"/> instance.
        /// </summary>
        /// <param name="token">The JWT token string to parse.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous parse operation. The task result contains the parsed <see cref="IJwtToken"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the token is null, empty, or not a valid JWT format.</exception>
        Task<IJwtToken> ParseAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Asynchronously attempts to parse a JWT token string into an <see cref="IJwtToken"/> instance.
        /// </summary>
        /// <param name="token">The JWT token string to parse.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous parse operation. The task result contains a tuple with a boolean indicating success and the parsed token (or null if unsuccessful).</returns>
        Task<(bool success, IJwtToken token)> TryParseAsync(string token, CancellationToken cancellationToken = default);
    }
} 