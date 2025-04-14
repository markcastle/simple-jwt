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
    /// <summary>
    /// Tests for JWT token security features, including replay attack prevention,
    /// token substitution prevention, and tampering detection.
    /// </summary>
    public class JwtTokenSecurityTests
    {
        private readonly IJwtValidator _validator;
        private readonly IJwtBuilder _builder;
        private readonly byte[] _hmacKey;
        private readonly HashSet<string> _usedJtis;

        public JwtTokenSecurityTests()
        {
            _validator = new JwtValidator(new JwtParser());
            _builder = new JwtBuilder();
            _hmacKey = Encoding.UTF8.GetBytes("secret-key");
            _usedJtis = new HashSet<string>();
        }

        /// <summary>
        /// Tests that a token cannot be reused after it has been validated (replay attack prevention).
        /// </summary>
        [Fact]
        public void ShouldPreventReplayAttack()
        {
            // Arrange
            string token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(30))
                .SetIssuedNow()
                .SetJwtId(Guid.NewGuid().ToString())
                .SignHs256(_hmacKey);

            var parameters = new ValidationParameters
            {
                ValidateJti = true,
                UsedJtis = _usedJtis,
                SymmetricSecurityKey = _hmacKey
            };

            // Act - First validation should succeed
            IJwtParser parser = new JwtParser();
            IJwtToken parsedToken = parser.Parse(token);
            ValidationResult firstResult = _validator.Validate(parsedToken, parameters);

            // Second validation should fail due to JTI being used
            ValidationResult secondResult = _validator.Validate(parsedToken, parameters);

            // Assert
            Assert.True(firstResult.IsValid);
            Assert.False(secondResult.IsValid);
            Assert.Equal(ValidationCodes.JtiAlreadyUsed, secondResult.Errors[0].Code);
        }

        /// <summary>
        /// Tests that a token cannot be substituted with a different token type.
        /// </summary>
        [Fact]
        public void ShouldPreventTokenSubstitution()
        {
            // Arrange
            string accessToken = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(30))
                .SetIssuedNow()
                .AddHeaderClaim(JwtConstants.HeaderType, "access")
                .SignHs256(_hmacKey);

            string refreshToken = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(30))
                .SetIssuedNow()
                .AddHeaderClaim(JwtConstants.HeaderType, "refresh")
                .SignHs256(_hmacKey);

            var parameters = new ValidationParameters
            {
                RequireTokenType = true,
                RequiredTokenType = "access",
                SymmetricSecurityKey = _hmacKey
            };

            // Act - Access token should pass
            IJwtParser parser = new JwtParser();
            IJwtToken accessTokenParsed = parser.Parse(accessToken);
            ValidationResult accessResult = _validator.Validate(accessTokenParsed, parameters);

            // Refresh token should fail
            IJwtToken refreshTokenParsed = parser.Parse(refreshToken);
            ValidationResult refreshResult = _validator.Validate(refreshTokenParsed, parameters);

            // Assert
            Assert.True(accessResult.IsValid);
            Assert.False(refreshResult.IsValid);
            Assert.Equal(ValidationCodes.InvalidClaimValue, refreshResult.Errors[0].Code);
        }

        /// <summary>
        /// Tests that signature tampering is detected during validation.
        /// </summary>
        [Fact]
        public void ShouldDetectSignatureTampering()
        {
            // Arrange
            string token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(30))
                .SetIssuedNow()
                .SignHs256(_hmacKey);

            // Create a different token with the same claims but a different key
            byte[] differentKey = new byte[_hmacKey.Length];
            Array.Copy(_hmacKey, differentKey, _hmacKey.Length);
            differentKey[0] ^= 0xFF; // Flip all bits in the first byte

            string tamperedToken = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(30))
                .SetIssuedNow()
                .SignHs256(differentKey);

            // Use the signature from the tampered token
            string[] parts = token.Split('.');
            string[] tamperedParts = tamperedToken.Split('.');
            string tamperedTokenWithOriginalHeader = $"{parts[0]}.{parts[1]}.{tamperedParts[2]}";

            var parameters = new ValidationParameters
            {
                ValidateSignature = true,
                SymmetricSecurityKey = _hmacKey
            };

            // Act
            IJwtParser parser = new JwtParser();
            IJwtToken parsedToken = parser.Parse(tamperedTokenWithOriginalHeader);
            ValidationResult result = _validator.Validate(parsedToken, parameters);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal(ValidationCodes.InvalidSignature, result.Errors[0].Code);
        }

        /// <summary>
        /// Tests that claim tampering is detected during validation.
        /// </summary>
        [Fact]
        public void ShouldDetectClaimTampering()
        {
            // Arrange
            string validToken = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(30))
                .SetIssuedNow()
                .SignHs256(_hmacKey);

            // Create a tampered token by modifying the subject claim
            string tamperedToken = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("malicious-subject") // Changed subject
                .SetExpiration(TimeSpan.FromMinutes(30))
                .SetIssuedNow()
                .CreateUnsecured(); // Create without signature

            // Use the signature from the valid token
            string[] validParts = validToken.Split('.');
            string[] tamperedParts = tamperedToken.Split('.');
            string tamperedTokenWithSignature = $"{tamperedParts[0]}.{tamperedParts[1]}.{validParts[2]}";

            var parameters = new ValidationParameters
            {
                ValidateSignature = true,
                SymmetricSecurityKey = _hmacKey
            };

            // Act
            IJwtParser parser = new JwtParser();
            IJwtToken parsedToken = parser.Parse(tamperedTokenWithSignature);
            ValidationResult result = _validator.Validate(parsedToken, parameters);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal(ValidationCodes.InvalidSignature, result.Errors[0].Code);
        }
    }
} 