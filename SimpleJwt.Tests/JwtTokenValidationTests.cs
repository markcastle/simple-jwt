using SimpleJwt.Abstractions;
using SimpleJwt.Abstractions.Validation;
using SimpleJwt.Core;
using System.Security.Cryptography;
using System.Text;

namespace SimpleJwt.Tests
{
    /// <summary>
    /// Tests for JWT token validation functionality, focusing on signature validation for different algorithms.
    /// </summary>
    public class JwtTokenValidationTests : TestBase
    {
        private readonly IJwtBuilder _builder;
        private readonly IJwtValidator _validator;
        private readonly byte[] _hmacKey;
        private readonly RSA _rsaKey;
        private readonly ECDsa _ecdsaKey;

        public JwtTokenValidationTests() : base(useSystemTextJson: true)
        {
            _builder = JwtBuilderFactory.Create();
            _validator = JwtValidatorFactory.Create();
            
            // Create test keys
            _hmacKey = new byte[32];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(_hmacKey);
            }

            _rsaKey = RSA.Create(2048);
            _ecdsaKey = ECDsa.Create();
        }

        /// <summary>
        /// Tests signature validation for HMAC-SHA256 algorithm.
        /// </summary>
        [Fact]
        public void ShouldValidateHmacSha256Signature()
        {
            // Arrange
            string token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(30))
                .SetIssuedNow()
                .SignHs256(_hmacKey);

            // Act
            ValidationResult result = _validator
                .SetHmacKey(_hmacKey)
                .Validate(token);

            // Assert
            Assert.True(result.IsValid);
            Assert.False(result.HasErrors);
        }

        /// <summary>
        /// Tests signature validation for HMAC-SHA384 algorithm.
        /// </summary>
        [Fact]
        public void ShouldValidateHmacSha384Signature()
        {
            // Arrange
            string token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(30))
                .SetIssuedNow()
                .SignHs384(_hmacKey);

            // Act
            ValidationResult result = _validator
                .SetHmacKey(_hmacKey)
                .Validate(token);

            // Assert
            Assert.True(result.IsValid);
            Assert.False(result.HasErrors);
        }

        /// <summary>
        /// Tests signature validation for HMAC-SHA512 algorithm.
        /// </summary>
        [Fact]
        public void ShouldValidateHmacSha512Signature()
        {
            // Arrange
            string token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(30))
                .SetIssuedNow()
                .SignHs512(_hmacKey);

            // Act
            ValidationResult result = _validator
                .SetHmacKey(_hmacKey)
                .Validate(token);

            // Assert
            Assert.True(result.IsValid);
            Assert.False(result.HasErrors);
        }

        /// <summary>
        /// Tests signature validation for RSA-SHA256 algorithm.
        /// </summary>
        [Fact]
        public void ShouldValidateRsaSha256Signature()
        {
            // Arrange
            string token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(30))
                .SetIssuedNow()
                .SignRs256(_rsaKey);

            var parameters = new ValidationParameters
            {
                RsaSecurityKey = _rsaKey,
                ValidateSignature = true
            };

            // Act
            IJwtParser parser = JwtParserFactory.Create();
            IJwtToken parsedToken = parser.Parse(token);
            ValidationResult result = _validator.Validate(parsedToken, parameters);

            // Assert
            Assert.True(result.IsValid);
            Assert.False(result.HasErrors);
        }

        /// <summary>
        /// Tests that signature validation fails when using the wrong key.
        /// </summary>
        [Fact]
        public void ShouldFailValidationWithWrongKey()
        {
            // Arrange
            byte[] wrongKey = new byte[32];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(wrongKey);
            }

            string token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(30))
                .SetIssuedNow()
                .SignHs256(_hmacKey);

            // Act
            ValidationResult result = _validator
                .SetHmacKey(wrongKey)
                .Validate(token);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrors);
            Assert.Equal(ValidationCodes.InvalidSignature, result.Errors[0].Code);
        }

        /// <summary>
        /// Tests that signature validation fails when the token is tampered with.
        /// </summary>
        [Fact]
        public void ShouldFailValidationWithTamperedToken()
        {
            // Arrange
            string token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(30))
                .SetIssuedNow()
                .SignHs256(_hmacKey);

            // Tamper with the token by modifying the payload
            string[] parts = token.Split('.');
            string tamperedPayload = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"iss\":\"tampered-issuer\",\"aud\":\"test-audience\",\"sub\":\"test-subject\",\"exp\":" + DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds() + "}"))
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
            string tamperedToken = $"{parts[0]}.{tamperedPayload}.{parts[2]}";

            // Act
            ValidationResult result = _validator
                .SetHmacKey(_hmacKey)
                .Validate(tamperedToken);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrors);
            Assert.Equal(ValidationCodes.InvalidSignature, result.Errors[0].Code);
        }

        /// <summary>
        /// Tests that signature validation fails when the algorithm is changed.
        /// </summary>
        [Fact]
        public void ShouldFailValidationWithChangedAlgorithm()
        {
            // Arrange
            string token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(30))
                .SetIssuedNow()
                .SignHs256(_hmacKey);

            // Change the algorithm in the header
            string[] parts = token.Split('.');
            string tamperedHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"alg\":\"HS384\",\"typ\":\"JWT\"}"))
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
            string tamperedToken = $"{tamperedHeader}.{parts[1]}.{parts[2]}";

            // Act
            ValidationResult result = _validator
                .SetHmacKey(_hmacKey)
                .Validate(tamperedToken);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrors);
            Assert.Equal(ValidationCodes.InvalidSignature, result.Errors[0].Code);
        }

        /// <summary>
        /// Tests that validation succeeds for a token that has not expired.
        /// </summary>
        [Fact]
        public void ShouldValidateNonExpiredToken()
        {
            // Arrange
            string token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(30)) // Token expires in 30 minutes
                .SetIssuedNow()
                .SignHs256(_hmacKey);

            var parameters = new ValidationParameters
            {
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            // Act
            IJwtParser parser = JwtParserFactory.Create();
            IJwtToken parsedToken = parser.Parse(token);
            ValidationResult result = _validator.Validate(parsedToken, parameters);

            // Assert
            Assert.True(result.IsValid);
            Assert.False(result.HasErrors);
        }

        /// <summary>
        /// Tests that validation fails for an expired token.
        /// </summary>
        [Fact]
        public void ShouldFailValidationForExpiredToken()
        {
            // Arrange
            string token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(-30)) // Token expired 30 minutes ago
                .SetIssuedNow()
                .SignHs256(_hmacKey);

            var parameters = new ValidationParameters
            {
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            // Act
            IJwtParser parser = JwtParserFactory.Create();
            IJwtToken parsedToken = parser.Parse(token);
            ValidationResult result = _validator.Validate(parsedToken, parameters);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrors);
            Assert.Equal(ValidationCodes.TokenExpired, result.Errors[0].Code);
        }

        /// <summary>
        /// Tests that validation succeeds for a token that has expired but is within the clock skew.
        /// </summary>
        [Fact]
        public void ShouldValidateTokenWithinClockSkew()
        {
            // Arrange
            string token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(-2)) // Token expired 2 minutes ago
                .SetIssuedNow()
                .SignHs256(_hmacKey);

            var parameters = new ValidationParameters
            {
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5) // 5 minutes clock skew
            };

            // Act
            IJwtParser parser = JwtParserFactory.Create();
            IJwtToken parsedToken = parser.Parse(token);
            ValidationResult result = _validator.Validate(parsedToken, parameters);

            // Assert
            Assert.True(result.IsValid);
            Assert.False(result.HasErrors);
        }

        /// <summary>
        /// Tests that validation succeeds when lifetime validation is disabled.
        /// </summary>
        [Fact]
        public void ShouldValidateExpiredTokenWhenLifetimeValidationDisabled()
        {
            // Arrange
            string token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(-30)) // Token expired 30 minutes ago
                .SetIssuedNow()
                .SignHs256(_hmacKey);

            var parameters = new ValidationParameters
            {
                ValidateLifetime = false // Disable lifetime validation
            };

            // Act
            IJwtParser parser = JwtParserFactory.Create();
            IJwtToken parsedToken = parser.Parse(token);
            ValidationResult result = _validator.Validate(parsedToken, parameters);

            // Assert
            Assert.True(result.IsValid);
            Assert.False(result.HasErrors);
        }

        /// <summary>
        /// Tests that validation fails for a token that is not yet valid (future nbf claim).
        /// </summary>
        [Fact]
        public void ShouldFailValidationForNotYetValidToken()
        {
            // Arrange
            var now = DateTime.UtcNow;
            string token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(30))
                .SetNotBefore(now.AddMinutes(10)) // Token becomes valid in 10 minutes
                .SignHs256(_hmacKey);

            var parameters = new ValidationParameters
            {
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            // Act
            IJwtParser parser = JwtParserFactory.Create();
            IJwtToken parsedToken = parser.Parse(token);
            ValidationResult result = _validator.Validate(parsedToken, parameters);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrors);
            Assert.Equal(ValidationCodes.TokenNotYetValid, result.Errors[0].Code);
        }
    }
} 