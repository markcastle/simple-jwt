using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SimpleJwt.Abstractions;
using SimpleJwt.Core.Utilities;

namespace SimpleJwt.Core
{
    /// <summary>
    /// Default implementation of <see cref="IJwtParser"/> for parsing JWT tokens.
    /// </summary>
    public class JwtParser : IJwtParser
    {
        /// <summary>
        /// Parses a JWT token into its component parts.
        /// </summary>
        /// <param name="token">The JWT token to parse.</param>
        /// <returns>A <see cref="IJwtToken"/> instance containing the parsed JWT token.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="token"/> is null or empty.</exception>
        /// <exception cref="FormatException">Thrown when the token is not a valid JWT token format.</exception>
        public IJwtToken Parse(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            var parts = token.Split('.');
            if (parts.Length != 3)
            {
                throw new FormatException("JWT token must contain three parts separated by dots.");
            }

            var header = JwtBase64UrlEncoder.Decode(parts[0]);
            var payload = JwtBase64UrlEncoder.Decode(parts[1]);

            try
            {
                var headerDict = JsonSerializer.Deserialize<Dictionary<string, object>>(header, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var payloadDict = JsonSerializer.Deserialize<Dictionary<string, object>>(payload, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return new JwtToken(headerDict, payloadDict, token);
            }
            catch (JsonException ex)
            {
                throw new FormatException("JWT token contains invalid JSON.", ex);
            }
        }

        /// <summary>
        /// Asynchronously parses a JWT token into its component parts.
        /// </summary>
        /// <param name="token">The JWT token to parse.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous parsing operation. The task result contains the parsed JWT token.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="token"/> is null or empty.</exception>
        /// <exception cref="FormatException">Thrown when the token is not a valid JWT token format.</exception>
        /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is canceled.</exception>
        public Task<IJwtToken> ParseAsync(string token, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(Parse(token));
        }

        /// <summary>
        /// Attempts to parse a JWT token into its component parts.
        /// </summary>
        /// <param name="token">The JWT token to parse.</param>
        /// <param name="result">When this method returns, contains the parsed JWT token, if parsing was successful, or null if parsing failed.</param>
        /// <returns>true if the token was successfully parsed; otherwise, false.</returns>
        public bool TryParse(string token, out IJwtToken result)
        {
            result = null;

            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            try
            {
                result = Parse(token);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Attempts to parse a JWT token into its component parts asynchronously.
        /// </summary>
        /// <param name="token">The JWT token to parse.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous parsing operation. The task result contains a tuple with a boolean indicating if parsing was successful and the parsed JWT token (if successful).</returns>
        /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is canceled.</exception>
        public async Task<(bool success, IJwtToken token)> TryParseAsync(string token, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(token))
            {
                return (false, null);
            }

            try
            {
                var result = await ParseAsync(token, cancellationToken).ConfigureAwait(false);
                return (true, result);
            }
            catch
            {
                return (false, null);
            }
        }
    }
} 