using SimpleJwt.Abstractions;
using SimpleJwt.Core;
using System.Security.Cryptography;
using System.Text.Json;
// ReSharper disable NotAccessedField.Local

namespace SimpleJwt.Tests
{
    /// <summary>
    /// Tests for JWT token creation functionality.
    /// </summary>
    public class JwtTokenCreationTests : TestBase
    {
        /// <summary>
        /// JWT builder used for creating tokens in tests.
        /// </summary>
        private readonly IJwtBuilder _jwtBuilder;

        /// <summary>
        /// JWT parser used for validating created tokens.
        /// </summary>
        private readonly IJwtParser _jwtParser;

        /// <summary>
        /// HMAC key used for signing tokens with symmetric algorithms.
        /// </summary>
        private readonly byte[] _hmacKey;

        /// <summary>
        /// RSA key used for signing tokens with RSA-based algorithms.
        /// </summary>
        private readonly RSA _rsaKey;

        /// <summary>
        /// ECDSA key used for signing tokens with elliptic curve algorithms.
        /// </summary>
        private readonly ECDsa _ecdsaKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtTokenCreationTests"/> class.
        /// Sets up the JWT builder, parser, and cryptographic keys for testing.
        /// </summary>
        public JwtTokenCreationTests() : base(useSystemTextJson: true)
        {
            // Initialize the JWT builder and parser
            _jwtBuilder = new JwtBuilder();
            _jwtParser = new JwtParser();

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
        /// Tests that a token can be created with all standard JWT claims and verified.
        /// </summary>
        [Fact]
        public void ShouldCreateTokenWithStandardClaims()
        {
            // Arrange
            DateTime now = DateTime.UtcNow;
            DateTime expiration = now.AddHours(1);

            // Act
            string token = _jwtBuilder
                .SetIssuer("https://test-issuer.com")
                .SetAudience("https://test-audience.com")
                .SetSubject("user123")
                .SetIssuedAt(now)
                .SetExpirationTime(expiration)
                .SetNotBefore(now)
                .SetJwtId("jti-123456")
                .SignHs256(_hmacKey);

            // Assert
            IJwtToken parsedToken = _jwtParser.Parse(token);
            Assert.Equal("https://test-issuer.com", parsedToken.GetClaim<string>(JwtConstants.ClaimIssuer));
            Assert.Equal("https://test-audience.com", parsedToken.GetClaim<string>(JwtConstants.ClaimAudience));
            Assert.Equal("user123", parsedToken.GetClaim<string>(JwtConstants.ClaimSubject));
            Assert.Equal("jti-123456", parsedToken.GetClaim<string>(JwtConstants.ClaimJwtId));
            Assert.Equal(JwtConstants.AlgorithmHs256, parsedToken.Header[JwtConstants.HeaderAlgorithm].ToString());

            // For numeric claims, we'll parse them as strings and convert manually
            long iat = long.Parse(parsedToken.GetClaim<string>(JwtConstants.ClaimIssuedAt));
            long exp = long.Parse(parsedToken.GetClaim<string>(JwtConstants.ClaimExpirationTime));
            long nbf = long.Parse(parsedToken.GetClaim<string>(JwtConstants.ClaimNotBefore));

            // Allow for a small time difference (up to 5 seconds) due to processing time
            long expectedIat = ((DateTimeOffset)now.ToUniversalTime()).ToUnixTimeSeconds();
            long expectedExp = ((DateTimeOffset)expiration.ToUniversalTime()).ToUnixTimeSeconds();
            long expectedNbf = ((DateTimeOffset)now.ToUniversalTime()).ToUnixTimeSeconds();

            Assert.True(Math.Abs(expectedIat - iat) <= 5, $"IssuedAt time difference is too large. Expected around {expectedIat}, got {iat}");
            Assert.True(Math.Abs(expectedExp - exp) <= 5, $"ExpirationTime difference is too large. Expected around {expectedExp}, got {exp}");
            Assert.True(Math.Abs(expectedNbf - nbf) <= 5, $"NotBefore time difference is too large. Expected around {expectedNbf}, got {nbf}");
        }

        /// <summary>
        /// Tests that tokens can be created with different HMAC signing algorithms.
        /// </summary>
        /// <param name="algorithm">The HMAC algorithm to test (HS256, HS384, or HS512).</param>
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
            IJwtToken parsedToken = _jwtParser.Parse(token);
            Assert.Equal(algorithm, parsedToken.Header[JwtConstants.HeaderAlgorithm].ToString());
            Assert.Equal("value", parsedToken.GetClaim<string>("test"));
        }

        /// <summary>
        /// Tests that tokens can be created with custom claims of various types.
        /// </summary>
        [Fact]
        public void ShouldCreateTokenWithCustomClaims()
        {
            // Arrange
            Dictionary<string, object> customClaims = new Dictionary<string, object>
            {
                { "role", "admin" },
                { "permissions", new[] { "read", "write" } },
                { "metadata", new { version = 1, active = true } }
            };

            // Act
            IJwtBuilder builder = _jwtBuilder;
            foreach (KeyValuePair<string, object> claim in customClaims)
            {
                builder = builder.AddClaim(claim.Key, claim.Value);
            }
            string token = builder.SignHs256(_hmacKey);

            // Assert
            IJwtToken parsedToken = _jwtParser.Parse(token);
            Assert.Equal("admin", parsedToken.GetClaim<string>("role"));

            // For array claims, we need to deserialize the JSON element
            string? permissionsJson = parsedToken.Payload["permissions"].ToString();
            string[]? permissions = JsonSerializer.Deserialize<string[]>(permissionsJson);
            Assert.Equal(new[] { "read", "write" }, permissions);

            // For object claims, we need to deserialize the JSON element
            string? metadataJson = parsedToken.Payload["metadata"].ToString();
            JsonElement metadata = JsonSerializer.Deserialize<JsonElement>(metadataJson);
            Assert.Equal(1, metadata.GetProperty("version").GetInt32());
            Assert.True(metadata.GetProperty("active").GetBoolean());
        }

        /// <summary>
        /// Tests that tokens can store deeply nested object structures as claims.
        /// </summary>
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
            string token = _jwtBuilder
                .AddClaim("data", nestedObject)
                .SignHs256(_hmacKey);

            // Assert
            IJwtToken parsedToken = _jwtParser.Parse(token);
            string? dataJson = parsedToken.Payload["data"].ToString();
            JsonElement data = JsonSerializer.Deserialize<JsonElement>(dataJson);

            Assert.Equal(123, data.GetProperty("user").GetProperty("id").GetInt32());
            Assert.Equal("John", data.GetProperty("user").GetProperty("profile").GetProperty("firstName").GetString());
            Assert.Equal("Doe", data.GetProperty("user").GetProperty("profile").GetProperty("lastName").GetString());
            Assert.Equal("dark", data.GetProperty("user").GetProperty("profile").GetProperty("preferences").GetProperty("theme").GetString());
            Assert.True(data.GetProperty("user").GetProperty("profile").GetProperty("preferences").GetProperty("notifications").GetBoolean());
        }

        /// <summary>
        /// Tests that empty values (strings, arrays, objects) can be properly stored in token claims.
        /// </summary>
        [Fact]
        public void ShouldHandleEmptyClaims()
        {
            // Act
            string token = _jwtBuilder
                .AddClaim("emptyString", "")
                .AddClaim("emptyArray", Array.Empty<string>())
                .AddClaim("emptyObject", new { })
                .SignHs256(_hmacKey);

            // Assert
            IJwtToken parsedToken = _jwtParser.Parse(token);
            Assert.Equal("", parsedToken.GetClaim<string>("emptyString"));

            // For array claims, we need to deserialize the JSON element
            string? emptyArrayJson = parsedToken.Payload["emptyArray"].ToString();
            string[]? emptyArray = JsonSerializer.Deserialize<string[]>(emptyArrayJson);
            Assert.Empty(emptyArray);

            // For object claims, we need to deserialize the JSON element
            string? emptyObjectJson = parsedToken.Payload["emptyObject"].ToString();
            JsonElement emptyObject = JsonSerializer.Deserialize<JsonElement>(emptyObjectJson);
            Assert.Equal(JsonValueKind.Object, emptyObject.ValueKind);
        }

        /// <summary>
        /// Tests that tokens can handle large data structures with numerous elements.
        /// </summary>
        [Fact]
        public void ShouldHandleLargeClaims()
        {
            // Arrange
            int[] largeArray = new int[1000];
            for (int i = 0; i < largeArray.Length; i++)
            {
                largeArray[i] = i;
            }

            // Act
            string token = _jwtBuilder
                .AddClaim("largeArray", largeArray)
                .SignHs256(_hmacKey);

            // Assert
            IJwtToken parsedToken = _jwtParser.Parse(token);
            string? largeArrayJson = parsedToken.Payload["largeArray"].ToString();
            int[]? parsedArray = JsonSerializer.Deserialize<int[]>(largeArrayJson);
            Assert.Equal(1000, parsedArray.Length);
            for (int i = 0; i < parsedArray.Length; i++)
            {
                Assert.Equal(i, parsedArray[i]);
            }
        }

        /// <summary>
        /// Tests that tokens can properly handle claims with special characters.
        /// </summary>
        [Fact]
        public void ShouldHandleSpecialCharacters()
        {
            // Arrange
            string specialChars = "!@#$%^&*()_+-=[]{}|;:'\",.<>?/\\";

            // Act
            string token = _jwtBuilder
                .AddClaim("special", specialChars)
                .SignHs256(_hmacKey);

            // Assert
            IJwtToken parsedToken = _jwtParser.Parse(token);
            Assert.Equal(specialChars, parsedToken.GetClaim<string>("special"));
        }

        /// <summary>
        /// Tests that tokens can properly handle Unicode characters in claims.
        /// </summary>
        [Fact]
        public void ShouldHandleUnicodeCharacters()
        {
            // Arrange
            string unicodeText = "Hello 世界 🌍";

            // Act
            string token = _jwtBuilder
                .AddClaim("unicode", unicodeText)
                .SignHs256(_hmacKey);

            // Assert
            IJwtToken parsedToken = _jwtParser.Parse(token);
            Assert.Equal(unicodeText, parsedToken.GetClaim<string>("unicode"));
        }
    }
}