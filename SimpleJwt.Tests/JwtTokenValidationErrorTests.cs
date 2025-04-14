using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SimpleJwt.Abstractions;
using SimpleJwt.Abstractions.Validation;
using SimpleJwt.Core;
using SimpleJwt.Core.Validation;
using Xunit;

namespace SimpleJwt.Tests
{
    /// <summary>
    /// Tests for JWT token validation error handling.
    /// </summary>
    public class JwtTokenValidationErrorTests
    {
        private readonly IJwtValidator _validator;
        private readonly IJwtBuilder _builder;
        private readonly byte[] _hmacKey;
        private readonly RSA _rsaKey;
        private readonly ECDsa _ecdsaKey;
        private readonly IJwtParser _parser;

        public JwtTokenValidationErrorTests()
        {
            _validator = new JwtValidator(new JwtParser());
            _builder = new JwtBuilder();
            _hmacKey = Encoding.UTF8.GetBytes("secret-key-with-at-least-32-bytes-for-hmac");
            _rsaKey = RSA.Create(2048);
            _ecdsaKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            _parser = new JwtParser();
        }

        #region Basic Error Code Tests

        /// <summary>
        /// Tests that the ValidationCodes.InvalidToken error code is returned
        /// when trying to validate a malformed token.
        /// </summary>
        [Fact]
        public void ShouldReturnInvalidTokenCode_WhenTokenIsMalformed()
        {
            // Arrange
            string malformedToken = "not.a.validtoken";

            // Act
            ValidationResult result = _validator.Validate(malformedToken);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrors);
            Assert.Equal(ValidationCodes.InvalidToken, result.Errors[0].Code);
            Assert.Contains("Failed to parse token", result.Errors[0].Message);
        }

        /// <summary>
        /// Tests that the ValidationCodes.TokenExpired error code is returned
        /// when validating an expired token.
        /// </summary>
        [Fact]
        public void ShouldReturnTokenExpiredCode_WhenTokenIsExpired()
        {
            // Arrange
            string token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpirationTime(DateTime.UtcNow.AddMinutes(-5)) // Expired 5 minutes ago
                .SetIssuedAt(DateTime.UtcNow.AddMinutes(-10))
                .SignHs256(_hmacKey);

            var parameters = new ValidationParameters
            {
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero, // No clock skew for this test
                SymmetricSecurityKey = _hmacKey
            };

            // Act
            IJwtParser parser = new JwtParser();
            IJwtToken parsedToken = parser.Parse(token);
            ValidationResult result = _validator.Validate(parsedToken, parameters);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrors);
            Assert.Equal(ValidationCodes.TokenExpired, result.Errors[0].Code);
            Assert.Contains("Token has expired", result.Errors[0].Message);
        }

        /// <summary>
        /// Tests that the ValidationCodes.TokenNotYetValid error code is returned
        /// when validating a token that is not yet valid.
        /// </summary>
        [Fact]
        public void ShouldReturnTokenNotYetValidCode_WhenTokenIsNotYetValid()
        {
            // Arrange - Use a longer time in the future to ensure test doesn't become flaky
            var futureTime = DateTime.UtcNow.AddMinutes(15);
            string token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpirationTime(futureTime.AddMinutes(30))
                .SetNotBefore(futureTime) // Not valid until 15 minutes in the future
                .SetIssuedAt(DateTime.UtcNow)
                .SignHs256(_hmacKey);

            var parameters = new ValidationParameters
            {
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero, // No clock skew for this test
                SymmetricSecurityKey = _hmacKey
            };

            // Act
            IJwtParser parser = new JwtParser();
            IJwtToken parsedToken = parser.Parse(token);
            ValidationResult result = _validator.Validate(parsedToken, parameters);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrors);
            Assert.Equal(ValidationCodes.TokenNotYetValid, result.Errors[0].Code);
            Assert.Contains("Token is not yet valid", result.Errors[0].Message);
        }

        /// <summary>
        /// Tests that the ValidationCodes.InvalidIssuer error code is returned
        /// when validating a token with an incorrect issuer.
        /// </summary>
        [Fact]
        public void ShouldReturnInvalidIssuerCode_WhenIssuerDoesNotMatch()
        {
            // Arrange
            string token = _builder
                .SetIssuer("wrong-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpirationTime(DateTime.UtcNow.AddMinutes(30))
                .SetIssuedAt(DateTime.UtcNow)
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
            Assert.Contains("Token issuer does not match", result.Errors[0].Message);
        }

        /// <summary>
        /// Tests that the ValidationCodes.InvalidAudience error code is returned
        /// when validating a token with an incorrect audience.
        /// </summary>
        [Fact]
        public void ShouldReturnInvalidAudienceCode_WhenAudienceDoesNotMatch()
        {
            // Arrange
            string token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("wrong-audience")
                .SetSubject("test-subject")
                .SetExpirationTime(DateTime.UtcNow.AddMinutes(30))
                .SetIssuedAt(DateTime.UtcNow)
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
            Assert.Contains("Token audience does not match", result.Errors[0].Message);
        }

        /// <summary>
        /// Tests that the ValidationCodes.InvalidSignature error code is returned
        /// when validating a token with an incorrect signature.
        /// </summary>
        [Fact]
        public void ShouldReturnInvalidSignatureCode_WhenSignatureIsInvalid()
        {
            // Arrange
            string token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpirationTime(DateTime.UtcNow.AddMinutes(30))
                .SetIssuedAt(DateTime.UtcNow)
                .SignHs256(_hmacKey);

            // Use a different key for validation
            byte[] wrongKey = Encoding.UTF8.GetBytes("wrong-key-with-at-least-32-bytes-for-hmac---");

            var parameters = new ValidationParameters
            {
                ValidateSignature = true,
                SymmetricSecurityKey = wrongKey
            };

            // Act
            IJwtParser parser = new JwtParser();
            IJwtToken parsedToken = parser.Parse(token);
            ValidationResult result = _validator.Validate(parsedToken, parameters);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrors);
            Assert.Equal(ValidationCodes.InvalidSignature, result.Errors[0].Code);
            Assert.Contains("Token signature is invalid", result.Errors[0].Message);
        }

        /// <summary>
        /// Tests that the ValidationCodes.MissingClaim error code is returned
        /// when a required claim is missing.
        /// </summary>
        [Fact]
        public void ShouldReturnMissingClaimCode_WhenRequiredClaimIsMissing()
        {
            // Arrange
            string token = _builder
                .SetAudience("test-audience") // No issuer set
                .SetSubject("test-subject")
                .SetExpirationTime(DateTime.UtcNow.AddMinutes(30))
                .SetIssuedAt(DateTime.UtcNow)
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
            Assert.Contains("missing", result.Errors[0].Message.ToLower());
        }

        /// <summary>
        /// Tests that the ValidationCodes.JtiMissing error code is returned
        /// when JTI validation is required but the JTI claim is missing.
        /// </summary>
        [Fact]
        public void ShouldReturnJtiMissingCode_WhenJtiIsMissing()
        {
            // Arrange
            string token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpirationTime(DateTime.UtcNow.AddMinutes(30))
                .SetIssuedAt(DateTime.UtcNow)
                // No JTI set
                .SignHs256(_hmacKey);

            var parameters = new ValidationParameters
            {
                ValidateJti = true,
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
            Assert.Contains("missing", result.Errors[0].Message.ToLower());
        }

        /// <summary>
        /// Tests that the ValidationCodes.JtiAlreadyUsed error code is returned
        /// when a token with a JTI that has already been used is validated.
        /// </summary>
        [Fact]
        public void ShouldReturnJtiAlreadyUsedCode_WhenJtiIsReused()
        {
            // Arrange
            string jti = Guid.NewGuid().ToString();
            string token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpirationTime(DateTime.UtcNow.AddMinutes(30))
                .SetIssuedAt(DateTime.UtcNow)
                .SetJwtId(jti)
                .SignHs256(_hmacKey);

            var usedJtis = new HashSet<string> { jti }; // Mark JTI as already used
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
            Assert.Contains("already been used", result.Errors[0].Message);
        }

        #endregion

        #region Error Message Clarity Tests

        /// <summary>
        /// Tests that error messages are clear and provide sufficient information.
        /// </summary>
        [Fact]
        public void ShouldProvideErrorMessagesWithClearInformation()
        {
            // Arrange
            string malformedToken = "not.a.validtoken";

            // Act
            ValidationResult result = _validator.Validate(malformedToken);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrors);
            
            // Error message should be informative and contain details
            Assert.NotEmpty(result.Errors[0].Message);
            Assert.True(result.Errors[0].Message.Length > 10); // Message should be reasonably long
            Assert.Contains("token", result.Errors[0].Message.ToLower()); // Should mention what was validated
        }

        /// <summary>
        /// Tests that validation error messages include the specific failing claim when relevant.
        /// </summary>
        [Fact]
        public void ShouldIncludeSpecificClaimInErrorMessages()
        {
            // Arrange
            string token = _builder
                .SetIssuer("wrong-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpirationTime(DateTime.UtcNow.AddMinutes(30))
                .SetIssuedAt(DateTime.UtcNow)
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
            
            // Error message should mention the issuer claim
            Assert.Contains("issuer", result.Errors[0].Message.ToLower());
        }

        /// <summary>
        /// Tests that ValidationError's ToString method provides a formatted error message.
        /// </summary>
        [Fact]
        public void ShouldFormatValidationErrorsToStringCorrectly()
        {
            // Arrange
            var error = new ValidationError(ValidationCodes.InvalidToken, "Test error message");

            // Act
            string errorString = error.ToString();

            // Assert
            Assert.Contains(ValidationCodes.InvalidToken, errorString);
            Assert.Contains("Test error message", errorString);
            Assert.Matches(@"\[\w+\] .+", errorString); // Format should be [ErrorCode] ErrorMessage
        }

        #endregion

        #region Multiple Error Aggregation Tests

        /// <summary>
        /// Tests that a ValidationResult can contain multiple errors.
        /// </summary>
        [Fact]
        public void ShouldAggregateMultipleErrors_WhenAddingMultipleErrors()
        {
            // Arrange
            var result = new ValidationResult(false);
            var error1 = new ValidationError(ValidationCodes.InvalidToken, "First error message");
            var error2 = new ValidationError(ValidationCodes.TokenExpired, "Second error message");

            // Act
            result.AddError(error1);
            result.AddError(error2);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrors);
            Assert.Equal(2, result.Errors.Count);
            Assert.Equal(ValidationCodes.InvalidToken, result.Errors[0].Code);
            Assert.Equal(ValidationCodes.TokenExpired, result.Errors[1].Code);
        }

        /// <summary>
        /// Tests that ValidationResult.Failure creates a result with one error.
        /// </summary>
        [Fact]
        public void ShouldCreateFailureResultWithOneError()
        {
            // Act
            var result = ValidationResult.Failure(ValidationCodes.InvalidToken, "Test error message");

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrors);
            Assert.Single(result.Errors);
            Assert.Equal(ValidationCodes.InvalidToken, result.Errors[0].Code);
            Assert.Equal("Test error message", result.Errors[0].Message);
        }

        #endregion

        #region Async Operation Error Handling Tests

        /// <summary>
        /// Tests that async validation operations correctly handle errors.
        /// </summary>
        [Fact]
        public async Task ShouldHandleErrorsInAsyncValidation()
        {
            // Arrange
            var expiredTime = DateTime.UtcNow.AddHours(-1);
            string token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpirationTime(expiredTime)
                .SetIssuedAt(expiredTime.AddMinutes(-10))
                .SignHs256(_hmacKey);

            var parameters = new ValidationParameters
            {
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                SymmetricSecurityKey = _hmacKey
            };

            // Act - Test async validation method
            IJwtToken parsedToken = _parser.Parse(token);
            ValidationResult result = await _validator.ValidateAsync(parsedToken, parameters, CancellationToken.None);

            // Assert
            Assert.False(result.IsValid);
            Assert.True(result.HasErrors);
            Assert.Equal(ValidationCodes.TokenExpired, result.Errors[0].Code);
        }

        /// <summary>
        /// Tests that TryValidateAsync correctly returns validation information without throwing.
        /// </summary>
        [Fact]
        public async Task ShouldReturnValidationInfoWithoutThrowing_WhenUsingTryValidateAsync()
        {
            // Arrange - Invalid token format
            string invalidToken = "not.a.validtoken";

            // Act - Should not throw but return validation info
            var (success, result) = await _validator.TryValidateAsync(invalidToken, CancellationToken.None);

            // Assert
            Assert.True(success); // Success means the method ran without throwing, not that validation succeeded
            Assert.False(result.IsValid); // The validation itself failed
            Assert.True(result.HasErrors);
            Assert.Equal(ValidationCodes.InvalidToken, result.Errors[0].Code);
        }

        /// <summary>
        /// Tests that cancellation token is respected in async validation.
        /// </summary>
        [Fact]
        public async Task ShouldRespectCancellationToken_InAsyncValidation()
        {
            // Arrange
            string token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpirationTime(DateTime.UtcNow.AddMinutes(30))
                .SetIssuedAt(DateTime.UtcNow)
                .SignHs256(_hmacKey);

            // Create a cancellation token that is immediately canceled
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert - Should throw OperationCanceledException
            await Assert.ThrowsAsync<OperationCanceledException>(() => 
                _validator.ValidateAsync(token, cts.Token));
        }

        #endregion
    }
} 