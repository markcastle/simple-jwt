using System;
using System.Text;
using Xunit;
using SimpleJwt.Abstractions;
using SimpleJwt.Abstractions.Serialization;
using SimpleJwt.Abstractions.Validation;
using SimpleJwt.Core;
using SimpleJwt.SystemTextJson.Serialization;

namespace SimpleJwt.Tests
{
    /// <summary>
    /// Provides basic tests for JWT token creation, parsing, and validation functionality.
    /// </summary>
    public class JwtBasicTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JwtBasicTests"/> class.
        /// Sets up the default implementations for JWT-related factories.
        /// </summary>
        public JwtBasicTests()
        {
            // Set up the JSON provider
            JsonProviderConfiguration.SetProvider(new SystemTextJsonProvider());
            
            // Initialize the factories with default implementations
            SimpleJwtDefaults.Initialize();
        }

        /// <summary>
        /// Tests that a JWT token can be successfully created and parsed.
        /// Verifies that all claims set during creation are correctly retrieved after parsing.
        /// </summary>
        [Fact]
        public void ShouldCreateAndParseToken()
        {
            // Arrange
            var builder = JwtBuilderFactory.Create();
            var parser = JwtParserFactory.Create();
            
            // Act
            string token = builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetId(Guid.NewGuid().ToString())
                .SetExpiration(TimeSpan.FromMinutes(30))
                .SetIssuedNow()
                .AddClaim("custom-claim", "custom-value")
                .CreateUnsecured();

            var jwtToken = parser.Parse(token);

            // Assert
            Assert.NotNull(jwtToken);
            Assert.Equal("test-issuer", jwtToken.GetClaim<string>(JwtConstants.ClaimIssuer));
            Assert.Equal("test-audience", jwtToken.GetClaim<string>(JwtConstants.ClaimAudience));
            Assert.Equal("test-subject", jwtToken.GetClaim<string>(JwtConstants.ClaimSubject));
            Assert.Equal("custom-value", jwtToken.GetClaim<string>("custom-claim"));
        }

        /// <summary>
        /// Tests that a signed JWT token can be successfully validated.
        /// Verifies that a correctly signed token with valid claims passes validation.
        /// </summary>
        [Fact]
        public void ShouldSignAndValidateToken()
        {
            // Arrange
            var builder = JwtBuilderFactory.Create();
            var validator = JwtValidatorFactory.Create();
            byte[] key = Encoding.UTF8.GetBytes("this-is-a-test-key-which-needs-to-be-at-least-32-bytes-long");
            
            // Act
            string token = builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(30))
                .SetIssuedNow()
                .SignHmacSha256(key);

            var result = validator
                .SetHmacKey(key)
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .ValidateExpiration()
                .Validate(token);

            // Assert
            Assert.True(result.IsValid);
            Assert.False(result.HasErrors);
        }

        /// <summary>
        /// Tests that validation fails when a JWT token is verified with an incorrect key.
        /// Verifies that the validation result contains the expected signature validation error.
        /// </summary>
        [Fact]
        public void ShouldFailValidationWithInvalidSignature()
        {
            // Arrange
            var builder = JwtBuilderFactory.Create();
            var validator = JwtValidatorFactory.Create();
            byte[] key = Encoding.UTF8.GetBytes("this-is-a-test-key-which-needs-to-be-at-least-32-bytes-long");
            byte[] wrongKey = Encoding.UTF8.GetBytes("this-is-a-wrong-key-which-is-also-at-least-32-bytes-long");
            
            // Act
            string token = builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(30))
                .SetIssuedNow()
                .SignHmacSha256(key);

            var result = validator
                .SetHmacKey(wrongKey)
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .ValidateExpiration()
                .Validate(token);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrors);
            Assert.Equal(ValidationCodes.InvalidSignature, result.Errors[0].Code);
        }

        /// <summary>
        /// Tests that validation fails when a JWT token contains an issuer that doesn't match the expected value.
        /// Verifies that the validation result contains the expected issuer validation error.
        /// </summary>
        [Fact]
        public void ShouldFailValidationWithWrongIssuer()
        {
            // Arrange
            var builder = JwtBuilderFactory.Create();
            var validator = JwtValidatorFactory.Create();
            byte[] key = Encoding.UTF8.GetBytes("this-is-a-test-key-which-needs-to-be-at-least-32-bytes-long");
            
            // Act
            string token = builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(30))
                .SetIssuedNow()
                .SignHmacSha256(key);

            var result = validator
                .SetHmacKey(key)
                .SetIssuer("wrong-issuer")
                .SetAudience("test-audience")
                .ValidateExpiration()
                .Validate(token);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrors);
            Assert.Equal(ValidationCodes.InvalidIssuer, result.Errors[0].Code);
        }

        /// <summary>
        /// Tests that validation fails when a JWT token has expired.
        /// Verifies that the validation result contains the expected token expiration error.
        /// </summary>
        [Fact]
        public void ShouldFailValidationWithExpiredToken()
        {
            // Arrange
            var builder = JwtBuilderFactory.Create();
            var validator = JwtValidatorFactory.Create();
            byte[] key = Encoding.UTF8.GetBytes("this-is-a-test-key-which-needs-to-be-at-least-32-bytes-long");
            
            // Act
            string token = builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(-10)) // Expired 10 minutes ago
                .SetIssuedNow()
                .SignHmacSha256(key);

            var result = validator
                .SetHmacKey(key)
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .ValidateExpiration()
                .SetClockSkew(TimeSpan.Zero) // No clock skew allowed
                .Validate(token);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrors);
            Assert.Equal(ValidationCodes.TokenExpired, result.Errors[0].Code);
        }
    }
} 