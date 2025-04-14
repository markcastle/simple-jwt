using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.Json;
using Xunit;
using SimpleJwt.Abstractions;
using SimpleJwt.Core;

namespace SimpleJwt.Tests
{
    /// <summary>
    /// Tests for JWT token creation functionality.
    /// </summary>
    public class JwtTokenCreationTests : TestBase
    {
        private readonly IJwtBuilder _jwtBuilder;
        private readonly IJwtParser _jwtParser;
        private readonly byte[] _hmacKey;
        private readonly RSA _rsaKey;
        private readonly ECDsa _ecdsaKey;

        public JwtTokenCreationTests() : base(useSystemTextJson: true)
        {
            // Initialize the JWT builder and parser
            _jwtBuilder = new JwtBuilder();
            _jwtParser = new JwtParser();
            
            // Create test keys
            _hmacKey = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(_hmacKey);
            }
            
            _rsaKey = RSA.Create(2048);
            _ecdsaKey = ECDsa.Create();
        }

        [Fact]
        public void ShouldCreateTokenWithStandardClaims()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var expiration = now.AddHours(1);
            
            // Act
            var token = _jwtBuilder
                .SetIssuer("https://test-issuer.com")
                .SetAudience("https://test-audience.com")
                .SetSubject("user123")
                .SetIssuedAt(now)
                .SetExpirationTime(expiration)
                .SetNotBefore(now)
                .SetJwtId("jti-123456")
                .SignHs256(_hmacKey);
            
            // Assert
            var parsedToken = _jwtParser.Parse(token);
            Assert.Equal("https://test-issuer.com", parsedToken.GetClaim<string>(JwtConstants.ClaimIssuer));
            Assert.Equal("https://test-audience.com", parsedToken.GetClaim<string>(JwtConstants.ClaimAudience));
            Assert.Equal("user123", parsedToken.GetClaim<string>(JwtConstants.ClaimSubject));
            Assert.Equal("jti-123456", parsedToken.GetClaim<string>(JwtConstants.ClaimJwtId));
            Assert.Equal(JwtConstants.AlgorithmHs256, parsedToken.Header[JwtConstants.HeaderAlgorithm].ToString());

            // For numeric claims, we'll parse them as strings and convert manually
            var iat = long.Parse(parsedToken.GetClaim<string>(JwtConstants.ClaimIssuedAt));
            var exp = long.Parse(parsedToken.GetClaim<string>(JwtConstants.ClaimExpirationTime));
            var nbf = long.Parse(parsedToken.GetClaim<string>(JwtConstants.ClaimNotBefore));

            // Allow for a small time difference (up to 5 seconds) due to processing time
            var expectedIat = ((DateTimeOffset)now.ToUniversalTime()).ToUnixTimeSeconds();
            var expectedExp = ((DateTimeOffset)expiration.ToUniversalTime()).ToUnixTimeSeconds();
            var expectedNbf = ((DateTimeOffset)now.ToUniversalTime()).ToUnixTimeSeconds();

            Assert.True(Math.Abs(expectedIat - iat) <= 5, $"IssuedAt time difference is too large. Expected around {expectedIat}, got {iat}");
            Assert.True(Math.Abs(expectedExp - exp) <= 5, $"ExpirationTime difference is too large. Expected around {expectedExp}, got {exp}");
            Assert.True(Math.Abs(expectedNbf - nbf) <= 5, $"NotBefore time difference is too large. Expected around {expectedNbf}, got {nbf}");
        }

        [Theory]
        [InlineData(JwtConstants.AlgorithmHs256)]
        [InlineData(JwtConstants.AlgorithmHs384)]
        [InlineData(JwtConstants.AlgorithmHs512)]
        public void ShouldCreateTokenWithDifferentHmacAlgorithms(string algorithm)
        {
            // Arrange
            string token;
            
            // Act
            switch (algorithm)
            {
                case JwtConstants.AlgorithmHs256:
                    token = _jwtBuilder.AddClaim("test", "value").SignHs256(_hmacKey);
                    break;
                case JwtConstants.AlgorithmHs384:
                    token = _jwtBuilder.AddClaim("test", "value").SignHs384(_hmacKey);
                    break;
                case JwtConstants.AlgorithmHs512:
                    token = _jwtBuilder.AddClaim("test", "value").SignHs512(_hmacKey);
                    break;
                default:
                    throw new ArgumentException($"Unsupported algorithm: {algorithm}");
            }
            
            // Assert
            var parsedToken = _jwtParser.Parse(token);
            Assert.Equal(algorithm, parsedToken.Header[JwtConstants.HeaderAlgorithm].ToString());
            Assert.Equal("value", parsedToken.GetClaim<string>("test"));
        }

        [Fact]
        public void ShouldCreateTokenWithCustomClaims()
        {
            // Arrange
            var customClaims = new Dictionary<string, object>
            {
                { "role", "admin" },
                { "permissions", new[] { "read", "write" } },
                { "metadata", new { version = 1, active = true } }
            };
            
            // Act
            var builder = _jwtBuilder;
            foreach (var claim in customClaims)
            {
                builder = builder.AddClaim(claim.Key, claim.Value);
            }
            var token = builder.SignHs256(_hmacKey);
            
            // Assert
            var parsedToken = _jwtParser.Parse(token);
            Assert.Equal("admin", parsedToken.GetClaim<string>("role"));

            // For array claims, we need to deserialize the JSON element
            var permissionsJson = parsedToken.Payload["permissions"].ToString();
            var permissions = JsonSerializer.Deserialize<string[]>(permissionsJson);
            Assert.Equal(new[] { "read", "write" }, permissions);

            // For object claims, we need to deserialize the JSON element
            var metadataJson = parsedToken.Payload["metadata"].ToString();
            var metadata = JsonSerializer.Deserialize<JsonElement>(metadataJson);
            Assert.Equal(1, metadata.GetProperty("version").GetInt32());
            Assert.True(metadata.GetProperty("active").GetBoolean());
        }

        [Fact]
        public void ShouldCreateTokenWithNestedObjects()
        {
            // Arrange
            var nestedObject = new
            {
                user = new
                {
                    id = 123,
                    profile = new
                    {
                        firstName = "John",
                        lastName = "Doe",
                        preferences = new
                        {
                            theme = "dark",
                            notifications = true
                        }
                    }
                }
            };
            
            // Act
            var token = _jwtBuilder
                .AddClaim("data", nestedObject)
                .SignHs256(_hmacKey);
            
            // Assert
            var parsedToken = _jwtParser.Parse(token);
            var dataJson = parsedToken.Payload["data"].ToString();
            var data = JsonSerializer.Deserialize<JsonElement>(dataJson);

            Assert.Equal(123, data.GetProperty("user").GetProperty("id").GetInt32());
            Assert.Equal("John", data.GetProperty("user").GetProperty("profile").GetProperty("firstName").GetString());
            Assert.Equal("Doe", data.GetProperty("user").GetProperty("profile").GetProperty("lastName").GetString());
            Assert.Equal("dark", data.GetProperty("user").GetProperty("profile").GetProperty("preferences").GetProperty("theme").GetString());
            Assert.True(data.GetProperty("user").GetProperty("profile").GetProperty("preferences").GetProperty("notifications").GetBoolean());
        }

        [Fact]
        public void ShouldHandleEmptyClaims()
        {
            // Act
            var token = _jwtBuilder
                .AddClaim("emptyString", "")
                .AddClaim("emptyArray", Array.Empty<string>())
                .AddClaim("emptyObject", new { })
                .SignHs256(_hmacKey);
            
            // Assert
            var parsedToken = _jwtParser.Parse(token);
            Assert.Equal("", parsedToken.GetClaim<string>("emptyString"));

            // For array claims, we need to deserialize the JSON element
            var emptyArrayJson = parsedToken.Payload["emptyArray"].ToString();
            var emptyArray = JsonSerializer.Deserialize<string[]>(emptyArrayJson);
            Assert.Empty(emptyArray);

            // For object claims, we need to deserialize the JSON element
            var emptyObjectJson = parsedToken.Payload["emptyObject"].ToString();
            var emptyObject = JsonSerializer.Deserialize<JsonElement>(emptyObjectJson);
            Assert.Equal(JsonValueKind.Object, emptyObject.ValueKind);
        }

        [Fact]
        public void ShouldHandleLargeClaims()
        {
            // Arrange
            var largeArray = new int[1000];
            for (int i = 0; i < largeArray.Length; i++)
            {
                largeArray[i] = i;
            }
            
            // Act
            var token = _jwtBuilder
                .AddClaim("largeArray", largeArray)
                .SignHs256(_hmacKey);
            
            // Assert
            var parsedToken = _jwtParser.Parse(token);
            var largeArrayJson = parsedToken.Payload["largeArray"].ToString();
            var parsedArray = JsonSerializer.Deserialize<int[]>(largeArrayJson);
            Assert.Equal(1000, parsedArray.Length);
            for (int i = 0; i < parsedArray.Length; i++)
            {
                Assert.Equal(i, parsedArray[i]);
            }
        }

        [Fact]
        public void ShouldHandleSpecialCharacters()
        {
            // Arrange
            var specialChars = "!@#$%^&*()_+-=[]{}|;:'\",.<>?/\\";
            
            // Act
            var token = _jwtBuilder
                .AddClaim("special", specialChars)
                .SignHs256(_hmacKey);
            
            // Assert
            var parsedToken = _jwtParser.Parse(token);
            Assert.Equal(specialChars, parsedToken.GetClaim<string>("special"));
        }

        [Fact]
        public void ShouldHandleUnicodeCharacters()
        {
            // Arrange
            var unicodeText = "Hello 世界 🌍";
            
            // Act
            var token = _jwtBuilder
                .AddClaim("unicode", unicodeText)
                .SignHs256(_hmacKey);
            
            // Assert
            var parsedToken = _jwtParser.Parse(token);
            Assert.Equal(unicodeText, parsedToken.GetClaim<string>("unicode"));
        }
    }
} 