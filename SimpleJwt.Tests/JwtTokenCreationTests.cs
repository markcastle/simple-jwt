using System;
using System.Text;
using System.Text.Json;
using Xunit;
using SimpleJwt.Abstractions;
using SimpleJwt.Abstractions.Serialization;
using SimpleJwt.Core;
using SimpleJwt.SystemTextJson.Serialization;

namespace SimpleJwt.Tests
{
    /// <summary>
    /// Provides comprehensive tests for JWT token creation functionality.
    /// </summary>
    public class JwtTokenCreationTests
    {
        private readonly IJwtBuilder _builder;
        private readonly IJwtParser _parser;
        private readonly byte[] _hmacKey;
        private readonly byte[] _rsaKey;

        public JwtTokenCreationTests()
        {
            // Set up the JSON provider
            JsonProviderConfiguration.SetProvider(new SystemTextJsonProvider());
            
            // Initialize the factories with default implementations
            SimpleJwtDefaults.Initialize();

            _builder = JwtBuilderFactory.Create();
            _parser = JwtParserFactory.Create();
            
            // Create test keys
            _hmacKey = Encoding.UTF8.GetBytes("this-is-a-test-key-which-needs-to-be-at-least-32-bytes-long");
            _rsaKey = Encoding.UTF8.GetBytes("this-is-an-rsa-key-which-needs-to-be-at-least-32-bytes-long");
        }

        [Fact]
        public void ShouldCreateTokenWithStandardClaims()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var expiration = now.AddHours(1);
            var notBefore = now.AddMinutes(5);
            var jti = Guid.NewGuid().ToString();

            // Act
            var token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetId(jti)
                .SetIssuedAt(now)
                .SetNotBefore(notBefore)
                .SetExpirationTime(expiration)
                .SignHmacSha256(_hmacKey);

            var parsedToken = _parser.Parse(token);

            // Assert
            Assert.NotNull(parsedToken);
            Assert.Equal("test-issuer", parsedToken.Payload[JwtConstants.ClaimIssuer].ToString());
            Assert.Equal("test-audience", parsedToken.Payload[JwtConstants.ClaimAudience].ToString());
            Assert.Equal("test-subject", parsedToken.Payload[JwtConstants.ClaimSubject].ToString());
            Assert.Equal(jti, parsedToken.Payload[JwtConstants.ClaimJwtId].ToString());
            Assert.Equal(new DateTimeOffset(now).ToUnixTimeSeconds(), long.Parse(parsedToken.Payload[JwtConstants.ClaimIssuedAt].ToString()));
            Assert.Equal(new DateTimeOffset(notBefore).ToUnixTimeSeconds(), long.Parse(parsedToken.Payload[JwtConstants.ClaimNotBefore].ToString()));
            Assert.Equal(new DateTimeOffset(expiration).ToUnixTimeSeconds(), long.Parse(parsedToken.Payload[JwtConstants.ClaimExpirationTime].ToString()));
        }

        [Theory]
        [InlineData(JwtConstants.AlgorithmHs256)]
        [InlineData(JwtConstants.AlgorithmHs384)]
        [InlineData(JwtConstants.AlgorithmHs512)]
        public void ShouldCreateTokenWithDifferentHmacAlgorithms(string algorithm)
        {
            // Act
            string token = null;
            var builder = _builder
                .SetIssuer("test-issuer")
                .SetSubject("test-subject");

            switch (algorithm)
            {
                case JwtConstants.AlgorithmHs256:
                    token = builder.SignHs256(_hmacKey);
                    break;
                case JwtConstants.AlgorithmHs384:
                    token = builder.SignHs384(_hmacKey);
                    break;
                case JwtConstants.AlgorithmHs512:
                    token = builder.SignHs512(_hmacKey);
                    break;
            }

            var parsedToken = _parser.Parse(token);

            // Assert
            Assert.NotNull(parsedToken);
            Assert.Equal(algorithm, parsedToken.Header[JwtConstants.HeaderAlgorithm].ToString());
        }

        [Fact]
        public void ShouldCreateTokenWithCustomClaims()
        {
            // Act
            string token = _builder
                .SetIssuer("test-issuer")
                .AddClaim("role", "admin")
                .AddClaim("permissions", new[] { "read", "write", "delete" })
                .AddClaim("metadata", new { version = 1, enabled = true })
                .SignHmacSha256(_hmacKey);

            var parsedToken = _parser.Parse(token);

            // Assert
            Assert.NotNull(parsedToken);
            Assert.Equal("admin", parsedToken.Payload["role"].ToString());
            
            var permissions = JsonSerializer.Deserialize<string[]>(
                parsedToken.Payload["permissions"].ToString());
            Assert.Equal(new[] { "read", "write", "delete" }, permissions);
            
            var metadata = JsonSerializer.Deserialize<JsonElement>(
                parsedToken.Payload["metadata"].ToString());
            Assert.Equal(1, metadata.GetProperty("version").GetInt32());
            Assert.True(metadata.GetProperty("enabled").GetBoolean());
        }

        [Fact]
        public void ShouldCreateTokenWithNestedObjects()
        {
            // Arrange
            var userInfo = new
            {
                id = 123,
                name = "Test User",
                preferences = new
                {
                    theme = "dark",
                    notifications = new
                    {
                        email = true,
                        push = false
                    }
                }
            };

            // Act
            string token = _builder
                .SetIssuer("test-issuer")
                .AddClaim("user", userInfo)
                .SignHmacSha256(_hmacKey);

            var parsedToken = _parser.Parse(token);

            // Assert
            Assert.NotNull(parsedToken);
            var user = JsonSerializer.Deserialize<JsonElement>(
                parsedToken.Payload["user"].ToString());
            Assert.Equal(123, user.GetProperty("id").GetInt32());
            Assert.Equal("Test User", user.GetProperty("name").GetString());
            Assert.Equal("dark", user.GetProperty("preferences").GetProperty("theme").GetString());
            Assert.True(user.GetProperty("preferences").GetProperty("notifications").GetProperty("email").GetBoolean());
            Assert.False(user.GetProperty("preferences").GetProperty("notifications").GetProperty("push").GetBoolean());
        }

        [Fact]
        public void ShouldHandleEmptyClaims()
        {
            // Act
            string token = _builder
                .SetIssuer("test-issuer")
                .AddClaim("empty-string", "")
                .AddClaim("empty-array", Array.Empty<string>())
                .AddClaim("empty-object", new { })
                .SignHmacSha256(_hmacKey);

            var parsedToken = _parser.Parse(token);

            // Assert
            Assert.NotNull(parsedToken);
            Assert.Equal("", parsedToken.Payload["empty-string"].ToString());
            
            var emptyArray = JsonSerializer.Deserialize<string[]>(
                parsedToken.Payload["empty-array"].ToString());
            Assert.Empty(emptyArray);
            
            var emptyObject = JsonSerializer.Deserialize<JsonElement>(
                parsedToken.Payload["empty-object"].ToString());
            Assert.Equal(0, emptyObject.EnumerateObject().Count());
        }

        [Fact]
        public void ShouldHandleLargeClaims()
        {
            // Arrange
            var largeArray = new string[1000];
            for (int i = 0; i < 1000; i++)
            {
                largeArray[i] = $"item-{i}";
            }

            // Act
            string token = _builder
                .SetIssuer("test-issuer")
                .AddClaim("large-array", largeArray)
                .SignHmacSha256(_hmacKey);

            var parsedToken = _parser.Parse(token);

            // Assert
            Assert.NotNull(parsedToken);
            var result = JsonSerializer.Deserialize<string[]>(
                parsedToken.Payload["large-array"].ToString());
            Assert.Equal(1000, result.Length);
            Assert.Equal("item-0", result[0]);
            Assert.Equal("item-999", result[999]);
        }

        [Fact]
        public void ShouldHandleSpecialCharacters()
        {
            // Arrange
            var specialChars = "!@#$%^&*()_+-=[]{}|;:'\",.<>?/\\`~";

            // Act
            string token = _builder
                .SetIssuer("test-issuer")
                .AddClaim("special-chars", specialChars)
                .SignHmacSha256(_hmacKey);

            var parsedToken = _parser.Parse(token);

            // Assert
            Assert.NotNull(parsedToken);
            Assert.Equal(specialChars, parsedToken.Payload["special-chars"].ToString());
        }

        [Fact]
        public void ShouldHandleUnicodeCharacters()
        {
            // Arrange
            var unicodeText = "Hello 世界 🌍";

            // Act
            string token = _builder
                .SetIssuer("test-issuer")
                .AddClaim("unicode-text", unicodeText)
                .SignHmacSha256(_hmacKey);

            var parsedToken = _parser.Parse(token);

            // Assert
            Assert.NotNull(parsedToken);
            Assert.Equal(unicodeText, parsedToken.Payload["unicode-text"].ToString());
        }
    }
} 