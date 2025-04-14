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
    public class JwtTokenValidationTests
    {
        private readonly IJwtValidator _validator;
        private readonly IJwtBuilder _builder;
        private readonly byte[] _hmacKey;

        public JwtTokenValidationTests()
        {
            _validator = new JwtValidator(new JwtParser());
            _builder = new JwtBuilder();
            _hmacKey = Encoding.UTF8.GetBytes("secret-key");
        }

        /// <summary>
        /// Tests that validation succeeds when issuer matches the expected value.
        /// </summary>
        [Fact]
        public void ShouldValidateMatchingIssuer()
        {
            // Arrange
            const string expectedIssuer = "test-issuer";
            string token = _builder
                .SetIssuer(expectedIssuer)
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(30))
                .SetIssuedNow()
                .SignHs256(_hmacKey);

            var parameters = new ValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = expectedIssuer,
                SymmetricSecurityKey = _hmacKey
            };

            // Act
            IJwtParser parser = new JwtParser();
            IJwtToken parsedToken = parser.Parse(token);
            ValidationResult result = _validator.Validate(parsedToken, parameters);

            // Assert
            Assert.True(result.IsValid);
            Assert.False(result.HasErrors);
        }

        /// <summary>
        /// Tests that validation fails when issuer doesn't match the expected value.
        /// </summary>
        [Fact]
        public void ShouldFailValidationWithWrongIssuer()
        {
            // Arrange
            string token = _builder
                .SetIssuer("wrong-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(30))
                .SetIssuedNow()
                .SignHs256(_hmacKey);

            var parameters = new ValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "expected-issuer",
                SymmetricSecurityKey = _hmacKey
            };

            // Act
            IJwtParser parser = new JwtParser();
            IJwtToken parsedToken = parser.Parse(token);
            ValidationResult result = _validator.Validate(parsedToken, parameters);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrors);
            Assert.Equal(ValidationCodes.InvalidIssuer, result.Errors[0].Code);
        }

        /// <summary>
        /// Tests that validation succeeds when audience matches the expected value.
        /// </summary>
        [Fact]
        public void ShouldValidateMatchingAudience()
        {
            // Arrange
            const string expectedAudience = "test-audience";
            string token = _builder
                .SetIssuer("test-issuer")
                .SetAudience(expectedAudience)
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(30))
                .SetIssuedNow()
                .SignHs256(_hmacKey);

            var parameters = new ValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = expectedAudience,
                SymmetricSecurityKey = _hmacKey
            };

            // Act
            IJwtParser parser = new JwtParser();
            IJwtToken parsedToken = parser.Parse(token);
            ValidationResult result = _validator.Validate(parsedToken, parameters);

            // Assert
            Assert.True(result.IsValid);
            Assert.False(result.HasErrors);
        }

        /// <summary>
        /// Tests that validation fails when audience doesn't match the expected value.
        /// </summary>
        [Fact]
        public void ShouldFailValidationWithWrongAudience()
        {
            // Arrange
            string token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("wrong-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(30))
                .SetIssuedNow()
                .SignHs256(_hmacKey);

            var parameters = new ValidationParameters
            {
                ValidateAudience = true,
                ValidAudience = "expected-audience",
                SymmetricSecurityKey = _hmacKey
            };

            // Act
            IJwtParser parser = new JwtParser();
            IJwtToken parsedToken = parser.Parse(token);
            ValidationResult result = _validator.Validate(parsedToken, parameters);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrors);
            Assert.Equal(ValidationCodes.InvalidAudience, result.Errors[0].Code);
        }

        /// <summary>
        /// Tests that validation succeeds when multiple audiences are accepted and token matches one.
        /// </summary>
        [Fact]
        public void ShouldValidateWithMultipleValidAudiences()
        {
            // Arrange
            const string tokenAudience = "audience2";
            string token = _builder
                .SetIssuer("test-issuer")
                .SetAudience(tokenAudience)
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(30))
                .SetIssuedNow()
                .SignHs256(_hmacKey);

            var parameters = new ValidationParameters
            {
                ValidateAudience = true,
                ValidAudiences = new[] { "audience1", tokenAudience, "audience3" },
                SymmetricSecurityKey = _hmacKey
            };

            // Act
            IJwtParser parser = new JwtParser();
            IJwtToken parsedToken = parser.Parse(token);
            ValidationResult result = _validator.Validate(parsedToken, parameters);

            // Assert
            Assert.True(result.IsValid);
            Assert.False(result.HasErrors);
        }

        /// <summary>
        /// Tests that validation succeeds when JTI claim is present and unique.
        /// </summary>
        [Fact]
        public void ShouldValidateUniqueJti()
        {
            // Arrange
            string jti = Guid.NewGuid().ToString();
            string token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(30))
                .SetIssuedNow()
                .AddClaim(JwtConstants.ClaimJwtId, jti)
                .SignHs256(_hmacKey);

            var parameters = new ValidationParameters
            {
                ValidateJti = true,
                UsedJtis = new HashSet<string>(),
                SymmetricSecurityKey = _hmacKey
            };

            // Act
            IJwtParser parser = new JwtParser();
            IJwtToken parsedToken = parser.Parse(token);
            ValidationResult result = _validator.Validate(parsedToken, parameters);

            // Assert
            Assert.True(result.IsValid);
            Assert.False(result.HasErrors);
        }

        /// <summary>
        /// Tests that validation fails when JTI claim has been used before (replay attack prevention).
        /// </summary>
        [Fact]
        public void ShouldFailValidationWithUsedJti()
        {
            // Arrange
            string jti = Guid.NewGuid().ToString();
            string token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(30))
                .SetIssuedNow()
                .AddClaim(JwtConstants.ClaimJwtId, jti)
                .SignHs256(_hmacKey);

            var usedJtis = new HashSet<string> { jti }; // Mark the JTI as already used
            var parameters = new ValidationParameters
            {
                ValidateJti = true,
                UsedJtis = usedJtis,
                SymmetricSecurityKey = _hmacKey
            };

            // Act
            IJwtParser parser = new JwtParser();
            IJwtToken parsedToken = parser.Parse(token);
            ValidationResult result = _validator.Validate(parsedToken, parameters);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrors);
            Assert.Equal(ValidationCodes.JtiAlreadyUsed, result.Errors[0].Code);
        }

        /// <summary>
        /// Tests that validation fails when JTI claim is missing but validation is required.
        /// </summary>
        [Fact]
        public void ShouldFailValidationWithMissingJti()
        {
            // Arrange
            string token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(30))
                .SetIssuedNow()
                .SignHs256(_hmacKey);

            var parameters = new ValidationParameters
            {
                ValidateJti = true,
                UsedJtis = new HashSet<string>(),
                SymmetricSecurityKey = _hmacKey
            };

            // Act
            IJwtParser parser = new JwtParser();
            IJwtToken parsedToken = parser.Parse(token);
            ValidationResult result = _validator.Validate(parsedToken, parameters);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrors);
            Assert.Equal(ValidationCodes.JtiMissing, result.Errors[0].Code);
        }
    }
}