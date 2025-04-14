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

            IJwtToken parsedToken = _parser.Parse(token);
            var parameters = new ValidationParameters
            {
                ValidateLifetime = _validateExpiration,
                ValidateIssuer = !string.IsNullOrEmpty(_issuer),
                ValidIssuer = _issuer,
                ValidateAudience = !string.IsNullOrEmpty(_audience),
                ValidAudience = _audience,
                ValidateSignature = true,
                ClockSkew = _clockSkew
            };

            return Validate(parsedToken, parameters);
        }

        /// <inheritdoc />
        public ValidationResult Validate(IJwtToken token, ValidationParameters parameters)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            var result = ValidationResult.Success();

            // Validate lifetime
            if (parameters.ValidateLifetime)
            {
                var expirationResult = ValidateExpirationTime(token);
                if (!expirationResult.IsValid)
                {
                    return expirationResult;
                }

                var notBeforeResult = ValidateNotBeforeTime(token);
                if (!notBeforeResult.IsValid)
                {
                    return notBeforeResult;
                }
            }

            // Validate issuer
            if (parameters.ValidateIssuer)
            {
                var issuerResult = ValidateIssuer(token, parameters);
                if (!issuerResult.IsValid)
                {
                    return issuerResult;
                }
            }

            // Validate audience
            if (parameters.ValidateAudience)
            {
                var audienceResult = ValidateAudience(token, parameters);
                if (!audienceResult.IsValid)
                {
                    return audienceResult;
                }
            }

            // Validate signature
            if (parameters.ValidateSignature)
            {
                ValidationResult signatureResult;
                
                // Check if we have a key ID in the header
                if (token.TryGetHeaderClaim<string>(JwtConstants.HeaderKeyId, out var keyId))
                {
                    // If we have a key ID, try to get the corresponding key from the SecurityKeys dictionary
                    if (parameters.SecurityKeys != null && parameters.SecurityKeys.TryGetValue(keyId, out var key))
                    {
                        if (key is byte[] symmetricKey)
                        {
                            signatureResult = ValidateSignature(token, symmetricKey);
                        }
                        else if (key is RSA rsaKey)
                        {
                            signatureResult = ValidateRsaSignature(token, rsaKey);
                        }
                        else if (key is ECDsa ecdsaKey)
                        {
                            signatureResult = ValidateEcdsaSignature(token, ecdsaKey);
                        }
                        else
                        {
                            signatureResult = ValidationResult.Failure(ValidationCodes.InvalidSignature, $"Unsupported key type for key ID: {keyId}");
                        }
                    }
                    else
                    {
                        signatureResult = ValidationResult.Failure(ValidationCodes.InvalidSignature, $"Key ID not found: {keyId}");
                    }
                }
                // If no key ID, fall back to the default key
                else if (parameters.SymmetricSecurityKey != null)
                {
                    signatureResult = ValidateSignature(token, parameters.SymmetricSecurityKey);
                }
                else if (parameters.RsaSecurityKey != null)
                {
                    signatureResult = ValidateRsaSignature(token, parameters.RsaSecurityKey);
                }
                else if (parameters.EcdsaSecurityKey != null)
                {
                    signatureResult = ValidateEcdsaSignature(token, parameters.EcdsaSecurityKey);
                }
                else if (_hmacKey != null)
                {
                    signatureResult = ValidateSignature(token, _hmacKey);
                }
                else
                {
                    signatureResult = ValidationResult.Failure(ValidationCodes.InvalidSignature, "No valid security key found for validation");
                }

                if (!signatureResult.IsValid)
                {
                    return signatureResult;
                }
            }

            // Validate JTI
            if (parameters.ValidateJti)
            {
                var jtiResult = ValidateJti(token, parameters);
                if (!jtiResult.IsValid)
                {
                    return jtiResult;
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
            _validators.Add(token => ValidateSignature(token, _hmacKey));
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
            _validators.Add(token => ValidateIssuer(token, new ValidationParameters { ValidIssuer = _issuer }));
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
            _validators.Add(token => ValidateAudience(token, new ValidationParameters { ValidAudience = _audience }));
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

        private ValidationResult ValidateIssuer(IJwtToken token, ValidationParameters parameters)
        {
            if (!token.Payload.TryGetValue(Abstractions.JwtConstants.ClaimIssuer, out object issuerClaim))
            {
                return ValidationResult.Failure(ValidationCodes.InvalidIssuer, $"Token is missing the required '{Abstractions.JwtConstants.ClaimIssuer}' claim.");
            }

            string issuer = issuerClaim?.ToString();
            if (string.IsNullOrEmpty(issuer))
            {
                return ValidationResult.Failure(ValidationCodes.InvalidIssuer, "Token issuer cannot be null or empty.");
            }

            if (!string.IsNullOrEmpty(parameters.ValidIssuer) && issuer != parameters.ValidIssuer)
            {
                return ValidationResult.Failure(ValidationCodes.InvalidIssuer, "Token issuer does not match the expected issuer.");
            }

            if (parameters.ValidIssuers?.Count > 0 && !parameters.ValidIssuers.Contains(issuer))
            {
                return ValidationResult.Failure(ValidationCodes.InvalidIssuer, "Token issuer is not in the list of valid issuers.");
            }

            return ValidationResult.Success();
        }

        private ValidationResult ValidateAudience(IJwtToken token, ValidationParameters parameters)
        {
            if (!token.Payload.TryGetValue(Abstractions.JwtConstants.ClaimAudience, out object audienceClaim))
            {
                return ValidationResult.Failure(ValidationCodes.InvalidAudience, $"Token is missing the required '{Abstractions.JwtConstants.ClaimAudience}' claim.");
            }

            string audience = audienceClaim?.ToString();
            if (string.IsNullOrEmpty(audience))
            {
                return ValidationResult.Failure(ValidationCodes.InvalidAudience, "Token audience cannot be null or empty.");
            }

            if (!string.IsNullOrEmpty(parameters.ValidAudience) && audience != parameters.ValidAudience)
            {
                return ValidationResult.Failure(ValidationCodes.InvalidAudience, "Token audience does not match the expected audience.");
            }

            if (parameters.ValidAudiences?.Count > 0 && !parameters.ValidAudiences.Contains(audience))
            {
                return ValidationResult.Failure(ValidationCodes.InvalidAudience, "Token audience is not in the list of valid audiences.");
            }

            return ValidationResult.Success();
        }

        private ValidationResult ValidateJti(IJwtToken token, ValidationParameters parameters)
        {
            if (!token.Payload.TryGetValue(Abstractions.JwtConstants.ClaimJwtId, out object jtiClaim))
            {
                return ValidationResult.Failure(ValidationCodes.JtiMissing, $"Token is missing the required '{Abstractions.JwtConstants.ClaimJwtId}' claim.");
            }

            string jti = jtiClaim?.ToString();
            if (string.IsNullOrEmpty(jti))
            {
                return ValidationResult.Failure(ValidationCodes.JtiMissing, "Token JTI cannot be null or empty.");
            }

            if (parameters.UsedJtis?.Contains(jti) == true)
            {
                return ValidationResult.Failure(ValidationCodes.JtiAlreadyUsed, "Token JTI has already been used.");
            }

            if (parameters.JtiValidator != null && !parameters.JtiValidator(jti))
            {
                return ValidationResult.Failure(ValidationCodes.InvalidClaimValue, "Token JTI failed custom validation.");
            }

            // Add the JTI to the used set if validation passed
            parameters.UsedJtis?.Add(jti);

            return ValidationResult.Success();
        }

        private ValidationResult ValidateSignature(IJwtToken token, byte[] key)
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
                return VerifyHmacSignature(token, SHA256.Create(), key);
            }
            else if (algorithm == Abstractions.JwtConstants.AlgorithmHs384)
            {
                return VerifyHmacSignature(token, SHA384.Create(), key);
            }
            else if (algorithm == Abstractions.JwtConstants.AlgorithmHs512)
            {
                return VerifyHmacSignature(token, SHA512.Create(), key);
            }

            return ValidationResult.Failure(ValidationCodes.InvalidSignature, $"Unsupported algorithm: {algorithm}");
        }

        private ValidationResult VerifyHmacSignature(IJwtToken token, HashAlgorithm hashAlgorithm, byte[] key)
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
            using (KeyedHashAlgorithm hmac = CreateHmacAlgorithm(algorithm, key))
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

        private KeyedHashAlgorithm CreateHmacAlgorithm(string algorithm, byte[] key)
        {
            if (key == null)
            {
                return null;
            }

            if (algorithm == Abstractions.JwtConstants.AlgorithmHs256)
            {
                return new HMACSHA256(key);
            }
            else if (algorithm == Abstractions.JwtConstants.AlgorithmHs384)
            {
                return new HMACSHA384(key);
            }
            else if (algorithm == Abstractions.JwtConstants.AlgorithmHs512)
            {
                return new HMACSHA512(key);
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

        private ValidationResult ValidateRsaSignature(IJwtToken token, RSA rsa)
        {
            if (!token.TryGetHeaderClaim<string>(JwtConstants.HeaderAlgorithm, out var algorithm))
            {
                return ValidationResult.Failure(ValidationCodes.InvalidSignature, "Token does not contain an algorithm header.");
            }

            if (algorithm == JwtConstants.AlgorithmNone)
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

            HashAlgorithmName hashAlgorithm = algorithm switch
            {
                JwtConstants.AlgorithmRs256 => HashAlgorithmName.SHA256,
                JwtConstants.AlgorithmRs384 => HashAlgorithmName.SHA384,
                JwtConstants.AlgorithmRs512 => HashAlgorithmName.SHA512,
                _ => throw new InvalidOperationException($"Unsupported RSA algorithm: {algorithm}")
            };

            bool isValid = rsa.VerifyData(dataBytes, signatureBytes, hashAlgorithm, RSASignaturePadding.Pkcs1);

            if (!isValid)
            {
                return ValidationResult.Failure(ValidationCodes.InvalidSignature, "Token signature is invalid.");
            }

            return ValidationResult.Success();
        }

        private ValidationResult ValidateEcdsaSignature(IJwtToken token, ECDsa ecdsa)
        {
            if (!token.TryGetHeaderClaim<string>(JwtConstants.HeaderAlgorithm, out var algorithm))
            {
                return ValidationResult.Failure(ValidationCodes.InvalidSignature, "Token does not contain an algorithm header.");
            }

            if (algorithm == JwtConstants.AlgorithmNone)
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

            HashAlgorithmName hashAlgorithm = algorithm switch
            {
                JwtConstants.AlgorithmEs256 => HashAlgorithmName.SHA256,
                JwtConstants.AlgorithmEs384 => HashAlgorithmName.SHA384,
                JwtConstants.AlgorithmEs512 => HashAlgorithmName.SHA512,
                _ => throw new InvalidOperationException($"Unsupported ECDSA algorithm: {algorithm}")
            };

            bool isValid = ecdsa.VerifyData(dataBytes, signatureBytes, hashAlgorithm);

            if (!isValid)
            {
                return ValidationResult.Failure(ValidationCodes.InvalidSignature, "Token signature is invalid.");
            }

            return ValidationResult.Success();
        }
    }
} 