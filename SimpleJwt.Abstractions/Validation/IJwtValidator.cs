using System;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleJwt.Abstractions.Validation
{
    /// <summary>
    /// Provides functionality to validate JWT tokens.
    /// </summary>
    public interface IJwtValidator
    {
        /// <summary>
        /// Validates a JWT token.
        /// </summary>
        /// <param name="token">The JWT token to validate.</param>
        /// <returns>A <see cref="ValidationResult"/> containing the result of the validation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="token"/> is null or empty.</exception>
        ValidationResult Validate(string token);

        /// <summary>
        /// Validates a JWT token.
        /// </summary>
        /// <param name="token">The JWT token to validate.</param>
        /// <param name="parameters">The validation parameters to use.</param>
        /// <returns>A <see cref="ValidationResult"/> containing the result of the validation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="token"/> is null.</exception>
        ValidationResult Validate(IJwtToken token, ValidationParameters parameters);

        /// <summary>
        /// Validates a JWT token asynchronously.
        /// </summary>
        /// <param name="token">The JWT token to validate.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous validation operation. The task result contains the validation result.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="token"/> is null or empty.</exception>
        /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is canceled.</exception>
        Task<ValidationResult> ValidateAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Validates a JWT token asynchronously.
        /// </summary>
        /// <param name="token">The JWT token to validate.</param>
        /// <param name="parameters">The validation parameters to use.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous validation operation. The task result contains the validation result.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="token"/> is null.</exception>
        /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is canceled.</exception>
        Task<ValidationResult> ValidateAsync(IJwtToken token, ValidationParameters parameters, CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to validate a JWT token.
        /// </summary>
        /// <param name="token">The JWT token to validate.</param>
        /// <param name="result">When this method returns, contains the validation result, if the validation was successful, or null if validation failed due to token formatting issues.</param>
        /// <returns>true if the validation completed (regardless of whether the token is valid); otherwise, false.</returns>
        bool TryValidate(string token, out ValidationResult result);

        /// <summary>
        /// Attempts to validate a JWT token asynchronously.
        /// </summary>
        /// <param name="token">The JWT token to validate.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous validation operation. The task result contains a tuple with a boolean indicating if validation completed and the validation result.</returns>
        /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is canceled.</exception>
        Task<(bool Success, ValidationResult Result)> TryValidateAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sets the HMAC key to use for signature validation.
        /// </summary>
        /// <param name="key">The HMAC key bytes.</param>
        /// <returns>The current <see cref="IJwtValidator"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null.</exception>
        IJwtValidator SetHmacKey(byte[] key);

        /// <summary>
        /// Sets the expected issuer of the JWT token.
        /// </summary>
        /// <param name="issuer">The expected issuer.</param>
        /// <returns>The current <see cref="IJwtValidator"/> instance.</returns>
        IJwtValidator SetIssuer(string issuer);

        /// <summary>
        /// Sets the expected audience of the JWT token.
        /// </summary>
        /// <param name="audience">The expected audience.</param>
        /// <returns>The current <see cref="IJwtValidator"/> instance.</returns>
        IJwtValidator SetAudience(string audience);

        /// <summary>
        /// Sets the clock skew to allow for when validating token expiration.
        /// </summary>
        /// <param name="clockSkew">The clock skew to allow.</param>
        /// <returns>The current <see cref="IJwtValidator"/> instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="clockSkew"/> is negative.</exception>
        IJwtValidator SetClockSkew(TimeSpan clockSkew);

        /// <summary>
        /// Configures the validator to validate token expiration.
        /// </summary>
        /// <param name="validate">Whether to validate token expiration.</param>
        /// <returns>The current <see cref="IJwtValidator"/> instance.</returns>
        IJwtValidator ValidateExpiration(bool validate = true);

        /// <summary>
        /// Configures the validator to validate token not-before time.
        /// </summary>
        /// <param name="validate">Whether to validate token not-before time.</param>
        /// <returns>The current <see cref="IJwtValidator"/> instance.</returns>
        IJwtValidator ValidateNotBefore(bool validate = true);
    }
} 