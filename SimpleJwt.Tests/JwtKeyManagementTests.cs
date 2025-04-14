using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using SimpleJwt.Abstractions;
using SimpleJwt.Abstractions.Validation;
using SimpleJwt.Core;
using SimpleJwt.Core.Validation;
using Xunit;

namespace SimpleJwt.Tests
{
    public class JwtKeyManagementTests
    {
        private readonly IJwtValidator _validator;
        private readonly IJwtBuilder _builder;
        private readonly IJwtParser _parser;
        private readonly Dictionary<string, byte[]> _keyStore;
        private readonly Dictionary<string, RSA> _rsaKeyStore;
        private readonly Dictionary<string, ECDsa> _ecdsaKeyStore;

        public JwtKeyManagementTests()
        {
            _validator = new JwtValidator(new JwtParser());
            _builder = new JwtBuilder();
            _parser = new JwtParser();
            _keyStore = new Dictionary<string, byte[]>
            {
                ["key-2023-04"] = Encoding.UTF8.GetBytes("key-2023-04-secret-key-with-at-least-32-bytes"),
                ["key-2023-05"] = Encoding.UTF8.GetBytes("key-2023-05-secret-key-with-at-least-32-bytes"),
                ["key-2023-06"] = Encoding.UTF8.GetBytes("key-2023-06-secret-key-with-at-least-32-bytes")
            };
            _rsaKeyStore = new Dictionary<string, RSA>
            {
                ["rsa-2023-04"] = RSA.Create(2048),
                ["rsa-2023-05"] = RSA.Create(2048),
                ["rsa-2023-06"] = RSA.Create(2048)
            };
            _ecdsaKeyStore = new Dictionary<string, ECDsa>
            {
                ["ecdsa-2023-04"] = ECDsa.Create(),
                ["ecdsa-2023-05"] = ECDsa.Create(),
                ["ecdsa-2023-06"] = ECDsa.Create()
            };
        }

        /// <summary>
        /// Tests that a token can be created and validated with a specific key ID.
        /// </summary>
        [Fact]
        public void ShouldCreateAndValidateTokenWithKeyId()
        {
            // Arrange
            const string keyId = "key-2023-05";
            string token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetKeyId(keyId)
                .SignHs256(_keyStore[keyId]);

            var parameters = new ValidationParameters
            {
                SecurityKeys = new Dictionary<string, object>
                {
                    [keyId] = _keyStore[keyId]
                },
                ValidateLifetime = false
            };

            // Act
            IJwtToken parsedToken = _parser.Parse(token);
            ValidationResult result = _validator.Validate(parsedToken, parameters);

            // Assert
            Assert.True(result.IsValid);
            Assert.False(result.HasErrors);
            Assert.True(parsedToken.TryGetHeaderClaim<string>(JwtConstants.HeaderKeyId, out var kid));
            Assert.Equal(keyId, kid);
        }

        /// <summary>
        /// Tests that key rotation works by validating tokens with different key IDs.
        /// </summary>
        [Fact]
        public void ShouldHandleKeyRotation()
        {
            // Arrange
            var parameters = new ValidationParameters
            {
                SecurityKeys = new Dictionary<string, object>
                {
                    ["key-2023-04"] = _keyStore["key-2023-04"],
                    ["key-2023-05"] = _keyStore["key-2023-05"],
                    ["key-2023-06"] = _keyStore["key-2023-06"]
                },
                ValidateLifetime = false
            };

            // Create tokens with different key IDs
            string token1 = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetKeyId("key-2023-04")
                .SignHs256(_keyStore["key-2023-04"]);

            string token2 = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetKeyId("key-2023-05")
                .SignHs256(_keyStore["key-2023-05"]);

            string token3 = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetKeyId("key-2023-06")
                .SignHs256(_keyStore["key-2023-06"]);

            // Act & Assert
            var result1 = _validator.Validate(_parser.Parse(token1), parameters);
            var result2 = _validator.Validate(_parser.Parse(token2), parameters);
            var result3 = _validator.Validate(_parser.Parse(token3), parameters);

            Assert.True(result1.IsValid);
            Assert.True(result2.IsValid);
            Assert.True(result3.IsValid);
        }

        /// <summary>
        /// Tests that validation fails when a token's key ID is not found in the key store.
        /// </summary>
        [Fact]
        public void ShouldFailValidationWithUnknownKeyId()
        {
            // Arrange
            const string unknownKeyId = "unknown-key";
            string token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetKeyId(unknownKeyId)
                .SignHs256(_keyStore["key-2023-05"]);

            var parameters = new ValidationParameters
            {
                SecurityKeys = new Dictionary<string, object>
                {
                    ["key-2023-05"] = _keyStore["key-2023-05"]
                },
                ValidateLifetime = false
            };

            // Act
            IJwtToken parsedToken = _parser.Parse(token);
            ValidationResult result = _validator.Validate(parsedToken, parameters);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrors);
            Assert.Equal(ValidationCodes.InvalidSignature, result.Errors[0].Code);
        }

        /// <summary>
        /// Tests that validation fails when a token's signature was created with a different key than specified by the key ID.
        /// </summary>
        [Fact]
        public void ShouldFailValidationWithMismatchedKeyId()
        {
            // Arrange
            const string keyId = "key-2023-05";
            // Create token with key-2023-05 but sign with key-2023-04
            string token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetKeyId(keyId)
                .SignHs256(_keyStore["key-2023-04"]); // Using wrong key

            var parameters = new ValidationParameters
            {
                SecurityKeys = new Dictionary<string, object>
                {
                    [keyId] = _keyStore["key-2023-05"]
                },
                ValidateLifetime = false
            };

            // Act
            IJwtToken parsedToken = _parser.Parse(token);
            ValidationResult result = _validator.Validate(parsedToken, parameters);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrors);
            Assert.Equal(ValidationCodes.InvalidSignature, result.Errors[0].Code);
        }

        /// <summary>
        /// Tests that validation fails when a token is missing the key ID header but key rotation is required.
        /// </summary>
        [Fact]
        public void ShouldFailValidationWithMissingKeyId()
        {
            // Arrange
            string token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SignHs256(_keyStore["key-2023-05"]); // No key ID set

            var parameters = new ValidationParameters
            {
                SecurityKeys = new Dictionary<string, object>
                {
                    ["key-2023-05"] = _keyStore["key-2023-05"]
                },
                ValidateLifetime = false
            };

            // Act
            IJwtToken parsedToken = _parser.Parse(token);
            ValidationResult result = _validator.Validate(parsedToken, parameters);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrors);
            Assert.Equal(ValidationCodes.InvalidSignature, result.Errors[0].Code);
        }

        /// <summary>
        /// Tests that multiple active keys can be used simultaneously for validation.
        /// </summary>
        [Fact]
        public void ShouldHandleMultipleActiveKeys()
        {
            // Arrange
            var parameters = new ValidationParameters
            {
                SecurityKeys = new Dictionary<string, object>
                {
                    ["key-2023-04"] = _keyStore["key-2023-04"],
                    ["key-2023-05"] = _keyStore["key-2023-05"]
                },
                ValidateLifetime = false
            };

            // Create tokens with different active keys
            string token1 = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetKeyId("key-2023-04")
                .SignHs256(_keyStore["key-2023-04"]);

            string token2 = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetKeyId("key-2023-05")
                .SignHs256(_keyStore["key-2023-05"]);

            // Act & Assert
            var result1 = _validator.Validate(_parser.Parse(token1), parameters);
            var result2 = _validator.Validate(_parser.Parse(token2), parameters);

            Assert.True(result1.IsValid);
            Assert.True(result2.IsValid);
        }

        /// <summary>
        /// Tests that a token can be created and validated with an RSA key ID.
        /// </summary>
        [Fact]
        public void ShouldCreateAndValidateTokenWithRsaKeyId()
        {
            // Arrange
            const string keyId = "rsa-2023-05";
            string token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetKeyId(keyId)
                .SignRs256(_rsaKeyStore[keyId]);

            var parameters = new ValidationParameters
            {
                SecurityKeys = new Dictionary<string, object>
                {
                    [keyId] = _rsaKeyStore[keyId]
                },
                ValidateLifetime = false
            };

            // Act
            IJwtToken parsedToken = _parser.Parse(token);
            ValidationResult result = _validator.Validate(parsedToken, parameters);

            // Assert
            Assert.True(result.IsValid);
            Assert.False(result.HasErrors);
            Assert.True(parsedToken.TryGetHeaderClaim<string>(JwtConstants.HeaderKeyId, out var kid));
            Assert.Equal(keyId, kid);
        }

        /// <summary>
        /// Tests that a token can be created and validated with an ECDSA key ID.
        /// </summary>
        [Fact]
        public void ShouldCreateAndValidateTokenWithEcdsaKeyId()
        {
            // Arrange
            const string keyId = "ecdsa-2023-05";
            string token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetKeyId(keyId)
                .SignEs256(_ecdsaKeyStore[keyId]);

            var parameters = new ValidationParameters
            {
                SecurityKeys = new Dictionary<string, object>
                {
                    [keyId] = _ecdsaKeyStore[keyId]
                },
                ValidateLifetime = false
            };

            // Act
            IJwtToken parsedToken = _parser.Parse(token);
            ValidationResult result = _validator.Validate(parsedToken, parameters);

            // Assert
            Assert.True(result.IsValid);
            Assert.False(result.HasErrors);
            Assert.True(parsedToken.TryGetHeaderClaim<string>(JwtConstants.HeaderKeyId, out var kid));
            Assert.Equal(keyId, kid);
        }

        /// <summary>
        /// Tests that key rotation works with mixed key types (HMAC, RSA, ECDSA).
        /// </summary>
        [Fact]
        public void ShouldHandleKeyRotationWithMixedKeyTypes()
        {
            // Arrange
            var parameters = new ValidationParameters
            {
                SecurityKeys = new Dictionary<string, object>
                {
                    ["key-2023-04"] = _keyStore["key-2023-04"],
                    ["rsa-2023-05"] = _rsaKeyStore["rsa-2023-05"],
                    ["ecdsa-2023-06"] = _ecdsaKeyStore["ecdsa-2023-06"]
                },
                ValidateLifetime = false
            };

            // Create tokens with different key types
            string token1 = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetKeyId("key-2023-04")
                .SignHs256(_keyStore["key-2023-04"]);

            string token2 = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetKeyId("rsa-2023-05")
                .SignRs256(_rsaKeyStore["rsa-2023-05"]);

            string token3 = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetKeyId("ecdsa-2023-06")
                .SignEs256(_ecdsaKeyStore["ecdsa-2023-06"]);

            // Act & Assert
            var result1 = _validator.Validate(_parser.Parse(token1), parameters);
            var result2 = _validator.Validate(_parser.Parse(token2), parameters);
            var result3 = _validator.Validate(_parser.Parse(token3), parameters);

            Assert.True(result1.IsValid);
            Assert.True(result2.IsValid);
            Assert.True(result3.IsValid);
        }

        /// <summary>
        /// Tests that validation fails when a token's key type doesn't match the expected type.
        /// </summary>
        [Fact]
        public void ShouldFailValidationWithWrongKeyType()
        {
            // Create a token with RSA key and RS256 algorithm
            var rsaKey = _rsaKeyStore["rsa-2023-05"];
            var token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetKeyId("rsa-2023-05")
                .SignRs256(rsaKey);

            // Try to validate with HMAC key
            var hmacKey = _keyStore["key-2023-05"];
            var parameters = new ValidationParameters
            {
                SecurityKeys = new Dictionary<string, object>
                {
                    { "key-2023-05", hmacKey }
                },
                ValidateSignature = true,
                ValidateLifetime = false
            };

            var parsedToken = _parser.Parse(token);
            var result = _validator.Validate(parsedToken, parameters);

            Assert.False(result.IsValid);
            Assert.True(result.HasErrors);
            Assert.Equal(ValidationCodes.InvalidSignature, result.Errors[0].Code);
        }
    }
} 