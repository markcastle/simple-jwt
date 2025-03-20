using System;
using System.Text;
using Xunit;
using SimpleJwt.Abstractions;
using SimpleJwt.Abstractions.Validation;
using SimpleJwt.Core;

namespace SimpleJwt.Tests
{
    public class JwtBasicTests
    {
        public JwtBasicTests()
        {
            // Initialize the factories with default implementations
            SimpleJwtDefaults.Initialize();
        }

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