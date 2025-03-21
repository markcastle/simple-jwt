using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SimpleJwt.Abstractions;
using SimpleJwt.Abstractions.Validation;
using SimpleJwt.Core.Utilities;

namespace SimpleJwt.Core.Validation
{
    /// <summary>
    /// Default implementation of <see cref="IJwtValidator"/> for validating JWT tokens.
    /// </summary>
    public class JwtValidator : IJwtValidator
    {
        private readonly IJwtParser _parser;
        private readonly List<Func<IJwtToken, ValidationResult>> _validators;
        private readonly List<Func<IJwtToken, Task<ValidationResult>>> _asyncValidators;
        private byte[] _hmacKey;
        private RSA _rsaPublicKey;
        private ECDsa _ecdsaPublicKey;
        private TimeSpan _clockSkew;
        private string _issuer;
        private string _audience;
        private bool _validateExpiration;
        private bool _validateNotBefore;

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtValidator"/> class.
        /// </summary>
        /// <param name="parser">The JWT parser to use for parsing tokens.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="parser"/> is null.</exception>
        public JwtValidator(IJwtParser parser)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _validators = new List<Func<IJwtToken, ValidationResult>>();
            _asyncValidators = new List<Func<IJwtToken, Task<ValidationResult>>>();
            _clockSkew = TimeSpan.FromMinutes(5);
            _validateExpiration = true;
            _validateNotBefore = true;
        }

        /// <inheritdoc />
        public ValidationResult Validate(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Token cannot be null or empty.", nameof(token));
            }

            try
            {
                IJwtToken parsedToken = _parser.Parse(token);
                return Validate(parsedToken, new ValidationParameters());
            }
            catch (Exception ex)
            {
                return ValidationResult.Failure(ValidationCodes.InvalidToken, $"Failed to parse token: {ex.Message}");
            }
        }

        /// <inheritdoc />
        public ValidationResult Validate(IJwtToken token, ValidationParameters parameters)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            ValidationResult result = ValidationResult.Success();

            // Validate expiration
            if (parameters.ValidateLifetime && _validateExpiration)
            {
                var expirationResult = ValidateExpirationTime(token);
                if (!expirationResult.IsValid)
                {
                    return expirationResult;
                }
            }

            // Validate not before
            if (parameters.ValidateLifetime && _validateNotBefore)
            {
                var notBeforeResult = ValidateNotBeforeTime(token);
                if (!notBeforeResult.IsValid)
                {
                    return notBeforeResult;
                }
            }

            // Validate issuer
            if (parameters.ValidateIssuer && !string.IsNullOrEmpty(_issuer))
            {
                var issuerResult = ValidateIssuer(token);
                if (!issuerResult.IsValid)
                {
                    return issuerResult;
                }
            }

            // Validate audience
            if (parameters.ValidateAudience && !string.IsNullOrEmpty(_audience))
            {
                var audienceResult = ValidateAudience(token);
                if (!audienceResult.IsValid)
                {
                    return audienceResult;
                }
            }

            // Validate signature
            if (parameters.ValidateSignature && _hmacKey != null)
            {
                var signatureResult = ValidateSignature(token);
                if (!signatureResult.IsValid)
                {
                    return signatureResult;
                }
            }

            // Run custom validators
            foreach (var validator in _validators)
            {
                var validationResult = validator(token);
                if (!validationResult.IsValid)
                {
                    return validationResult;
                }
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<ValidationResult> ValidateAsync(string token, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Token cannot be null or empty.", nameof(token));
            }

            try
            {
                IJwtToken parsedToken = _parser.Parse(token);
                return await ValidateAsync(parsedToken, new ValidationParameters(), cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return ValidationResult.Failure(ValidationCodes.InvalidToken, $"Failed to parse token: {ex.Message}");
            }
        }

        /// <inheritdoc />
        public async Task<ValidationResult> ValidateAsync(IJwtToken token, ValidationParameters parameters, CancellationToken cancellationToken = default)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            // First perform synchronous validations
            ValidationResult result = Validate(token, parameters);
            if (!result.IsValid)
            {
                return result;
            }

            // Run async validators
            foreach (var validator in _asyncValidators)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var validationResult = await validator(token).ConfigureAwait(false);
                if (!validationResult.IsValid)
                {
                    return validationResult;
                }
            }

            return result;
        }

        /// <inheritdoc />
        public bool TryValidate(string token, out ValidationResult result)
        {
            if (string.IsNullOrEmpty(token))
            {
                result = ValidationResult.Failure(ValidationCodes.InvalidToken, "Token cannot be null or empty.");
                return false;
            }

            try
            {
                IJwtToken parsedToken = _parser.Parse(token);
                result = Validate(parsedToken, new ValidationParameters());
                return true;
            }
            catch (Exception ex)
            {
                result = ValidationResult.Failure(ValidationCodes.InvalidToken, $"Failed to parse token: {ex.Message}");
                return false;
            }
        }

        /// <inheritdoc />
        public async Task<(bool Success, ValidationResult Result)> TryValidateAsync(string token, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(token))
            {
                return (false, ValidationResult.Failure(ValidationCodes.InvalidToken, "Token cannot be null or empty."));
            }

            try
            {
                IJwtToken parsedToken = _parser.Parse(token);
                ValidationResult result = await ValidateAsync(parsedToken, new ValidationParameters(), cancellationToken).ConfigureAwait(false);
                return (true, result);
            }
            catch (Exception ex)
            {
                return (false, ValidationResult.Failure(ValidationCodes.InvalidToken, $"Failed to parse token: {ex.Message}"));
            }
        }

        /// <summary>
        /// Sets the HMAC key to use for signature validation.
        /// </summary>
        /// <param name="key">The key to use for validation.</param>
        /// <returns>The current <see cref="IJwtValidator"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null.</exception>
        public IJwtValidator SetHmacKey(byte[] key)
        {
            _hmacKey = key ?? throw new ArgumentNullException(nameof(key));
            _validators.Add(ValidateSignature);
            return this;
        }

        /// <summary>
        /// Sets the expected issuer of the JWT token.
        /// </summary>
        /// <param name="issuer">The expected issuer.</param>
        /// <returns>The current <see cref="IJwtValidator"/> instance.</returns>
        public IJwtValidator SetIssuer(string issuer)
        {
            _issuer = issuer;
            _validators.Add(ValidateIssuer);
            return this;
        }

        /// <summary>
        /// Sets the expected audience of the JWT token.
        /// </summary>
        /// <param name="audience">The expected audience.</param>
        /// <returns>The current <see cref="IJwtValidator"/> instance.</returns>
        public IJwtValidator SetAudience(string audience)
        {
            _audience = audience;
            _validators.Add(ValidateAudience);
            return this;
        }

        /// <summary>
        /// Sets the clock skew to allow for when validating token expiration.
        /// </summary>
        /// <param name="clockSkew">The clock skew to allow.</param>
        /// <returns>The current <see cref="IJwtValidator"/> instance.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="clockSkew"/> is negative.</exception>
        public IJwtValidator SetClockSkew(TimeSpan clockSkew)
        {
            if (clockSkew < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(clockSkew), "Clock skew cannot be negative.");
            }

            _clockSkew = clockSkew;
            return this;
        }

        /// <summary>
        /// Configures the validator to validate token expiration.
        /// </summary>
        /// <param name="validate">Whether to validate token expiration.</param>
        /// <returns>The current <see cref="IJwtValidator"/> instance.</returns>
        public IJwtValidator ValidateExpiration(bool validate = true)
        {
            _validateExpiration = validate;
            if (validate)
            {
                _validators.Add(ValidateExpirationTime);
            }
            return this;
        }

        /// <summary>
        /// Configures the validator to validate token not-before time.
        /// </summary>
        /// <param name="validate">Whether to validate token not-before time.</param>
        /// <returns>The current <see cref="IJwtValidator"/> instance.</returns>
        public IJwtValidator ValidateNotBefore(bool validate = true)
        {
            _validateNotBefore = validate;
            if (validate)
            {
                _validators.Add(ValidateNotBeforeTime);
            }
            return this;
        }

        private ValidationResult ValidateSignature(IJwtToken token)
        {
            string algorithm = token.Header.GetStringOrDefault(Abstractions.JwtConstants.HeaderAlgorithm, string.Empty);

            // No algorithm specified
            if (string.IsNullOrEmpty(algorithm))
            {
                return ValidationResult.Failure(ValidationCodes.InvalidSignature, "No algorithm specified in token header.");
            }

            // None algorithm (no signature)
            if (algorithm == Abstractions.JwtConstants.AlgorithmNone)
            {
                // If using the 'none' algorithm, verify there's no signature
                if (!string.IsNullOrEmpty(token.Signature))
                {
                    return ValidationResult.Failure(ValidationCodes.InvalidSignature, "Token uses 'none' algorithm but has a signature.");
                }

                return ValidationResult.Success();
            }

            // Verify the signature is present
            if (string.IsNullOrEmpty(token.Signature))
            {
                return ValidationResult.Failure(ValidationCodes.InvalidSignature, "Token is missing a signature.");
            }

            // HMAC algorithms
            if (algorithm == Abstractions.JwtConstants.AlgorithmHs256)
            {
                return VerifyHmacSignature(token, SHA256.Create());
            }
            else if (algorithm == Abstractions.JwtConstants.AlgorithmHs384)
            {
                return VerifyHmacSignature(token, SHA384.Create());
            }
            else if (algorithm == Abstractions.JwtConstants.AlgorithmHs512)
            {
                return VerifyHmacSignature(token, SHA512.Create());
            }

            // ... other algorithm verification ...

            return ValidationResult.Failure(ValidationCodes.InvalidSignature, $"Unsupported algorithm: {algorithm}");
        }

        private ValidationResult ValidateIssuer(IJwtToken token)
        {
            if (!token.Payload.TryGetValue(Abstractions.JwtConstants.ClaimIssuer, out object issuerClaim))
            {
                return ValidationResult.Failure(ValidationCodes.InvalidIssuer, $"Token is missing the required '{Abstractions.JwtConstants.ClaimIssuer}' claim.");
            }

            string issuer = issuerClaim?.ToString();
            if (string.IsNullOrEmpty(issuer) || issuer != _issuer)
            {
                return ValidationResult.Failure(ValidationCodes.InvalidIssuer, "Token issuer does not match the expected issuer.");
            }

            return ValidationResult.Success();
        }

        private ValidationResult ValidateAudience(IJwtToken token)
        {
            if (!token.Payload.TryGetValue(Abstractions.JwtConstants.ClaimAudience, out object audienceClaim))
            {
                return ValidationResult.Failure(ValidationCodes.InvalidAudience, $"Token is missing the required '{Abstractions.JwtConstants.ClaimAudience}' claim.");
            }

            string audience = audienceClaim?.ToString();
            if (string.IsNullOrEmpty(audience) || audience != _audience)
            {
                return ValidationResult.Failure(ValidationCodes.InvalidAudience, "Token audience does not match the expected audience.");
            }

            return ValidationResult.Success();
        }

        private ValidationResult ValidateExpirationTime(IJwtToken token)
        {
            if (!token.Payload.TryGetValue(Abstractions.JwtConstants.ClaimExpirationTime, out object expirationClaim))
            {
                return ValidationResult.Failure(ValidationCodes.TokenExpired, $"Token is missing the required '{Abstractions.JwtConstants.ClaimExpirationTime}' claim.");
            }

            if (!long.TryParse(expirationClaim?.ToString(), out long expirationTime))
            {
                return ValidationResult.Failure(ValidationCodes.TokenExpired, "Invalid expiration time format.");
            }

            DateTimeOffset expirationDateTime = DateTimeOffset.FromUnixTimeSeconds(expirationTime);
            if (DateTimeOffset.UtcNow > expirationDateTime.Add(_clockSkew))
            {
                return ValidationResult.Failure(ValidationCodes.TokenExpired, "Token has expired.");
            }

            return ValidationResult.Success();
        }

        private ValidationResult ValidateNotBeforeTime(IJwtToken token)
        {
            if (!token.Payload.TryGetValue(Abstractions.JwtConstants.ClaimNotBefore, out object notBeforeClaim))
            {
                // Not before is optional
                return ValidationResult.Success();
            }

            if (!long.TryParse(notBeforeClaim?.ToString(), out long notBeforeTime))
            {
                return ValidationResult.Failure(ValidationCodes.TokenNotYetValid, "Invalid not-before time format.");
            }

            DateTimeOffset notBeforeDateTime = DateTimeOffset.FromUnixTimeSeconds(notBeforeTime);
            if (DateTimeOffset.UtcNow < notBeforeDateTime.Subtract(_clockSkew))
            {
                return ValidationResult.Failure(ValidationCodes.TokenNotYetValid, "Token is not yet valid.");
            }

            return ValidationResult.Success();
        }

        private ValidationResult VerifyHmacSignature(IJwtToken token, HashAlgorithm hashAlgorithm)
        {
            if (!token.TryGetHeaderClaim<string>(Abstractions.JwtConstants.HeaderAlgorithm, out var algorithm))
            {
                return ValidationResult.Failure(ValidationCodes.InvalidSignature, "Token does not contain an algorithm header.");
            }

            if (algorithm == Abstractions.JwtConstants.AlgorithmNone)
            {
                return ValidationResult.Failure(ValidationCodes.InvalidSignature, "Token uses 'none' algorithm but has a signature.");
            }

            var parts = token.RawToken.Split('.');
            if (parts.Length != 3)
            {
                return ValidationResult.Failure(ValidationCodes.InvalidToken, "Token must contain three parts separated by dots.");
            }

            string data = $"{parts[0]}.{parts[1]}";
            string signature = parts[2];

            byte[] signatureBytes = JwtBase64UrlEncoder.DecodeToBytes(signature);
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            bool isValid = false;

            // Use the appropriate HMAC algorithm based on the hash algorithm
            using (KeyedHashAlgorithm hmac = CreateHmacAlgorithm(algorithm))
            {
                if (hmac != null)
                {
                    byte[] hash = hmac.ComputeHash(dataBytes);
                    isValid = CompareBytes(hash, signatureBytes);
                }
            }

            if (!isValid)
            {
                return ValidationResult.Failure(ValidationCodes.InvalidSignature, "Token signature is invalid.");
            }

            return ValidationResult.Success();
        }

        private KeyedHashAlgorithm CreateHmacAlgorithm(string algorithm)
        {
            if (_hmacKey == null)
            {
                return null;
            }

            if (algorithm == Abstractions.JwtConstants.AlgorithmHs256)
            {
                return new HMACSHA256(_hmacKey);
            }
            else if (algorithm == Abstractions.JwtConstants.AlgorithmHs384)
            {
                return new HMACSHA384(_hmacKey);
            }
            else if (algorithm == Abstractions.JwtConstants.AlgorithmHs512)
            {
                return new HMACSHA512(_hmacKey);
            }

            return null;
        }

        private static bool CompareBytes(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            uint result = 0;
            for (int i = 0; i < a.Length; i++)
            {
                result |= (uint)(a[i] ^ b[i]);
            }
            
            return result == 0;
        }
    }
} 