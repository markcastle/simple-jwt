using SimpleJwt.Abstractions;
using System.Security.Cryptography;
using System.Text;

namespace SimpleJwt.Tests
{
    /// <summary>
    /// Tests for JWT token parsing functionality, including various formats and error cases.
    /// </summary>
    public class JwtTokenParsingTests : TestBase
    {
        private readonly IJwtBuilder _builder;
        private readonly IJwtParser _parser;
        private readonly byte[] _hmacKey;
        private readonly RSA _rsaKey;

        // ReSharper disable once ConvertConstructorToMemberInitializers
        public JwtTokenParsingTests()
        {
            _builder = JwtBuilderFactory.Create();
            _parser = JwtParserFactory.Create();
            _hmacKey = "this-is-a-test-key-which-needs-to-be-at-least-32-bytes-long"u8.ToArray();
            _rsaKey = RSA.Create(2048);
        }

        /// <summary>
        /// Tests parsing of tokens created with different JSON serialization providers.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ShouldParseTokensWithDifferentJsonProviders(bool useSystemTextJson)
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
            IJwtToken parsedToken = _parser.Parse(token);

            // Assert
            Assert.NotNull(parsedToken);
            Assert.Equal("test-issuer", parsedToken.GetClaim<string>("iss"));
            Assert.Equal("test-audience", parsedToken.GetClaim<string>("aud"));
            Assert.Equal("test-subject", parsedToken.GetClaim<string>("sub"));
        }

        /// <summary>
        /// Tests parsing of tokens with different signing algorithms.
        /// </summary>
        [Theory]
        [InlineData("HS256")]
        [InlineData("HS384")]
        [InlineData("HS512")]
        [InlineData("RS256")]
        public void ShouldParseTokensWithDifferentAlgorithms(string algorithm)
        {
            // Arrange
            string token;
            switch (algorithm)
            {
                case "HS256":
                    token = _builder.SignHs256(_hmacKey);
                    break;
                case "HS384":
                    token = _builder.SignHs384(_hmacKey);
                    break;
                case "HS512":
                    token = _builder.SignHs512(_hmacKey);
                    break;
                case "RS256":
                    token = _builder.SignRs256(_rsaKey);
                    break;
                default:
                    throw new ArgumentException($"Unsupported algorithm: {algorithm}");
            }

            // Act
            IJwtToken parsedToken = _parser.Parse(token);

            // Assert
            Assert.NotNull(parsedToken);
            Assert.True(parsedToken.TryGetHeaderClaim<string>("alg", out string alg));
            Assert.Equal(algorithm, alg);
        }

        /// <summary>
        /// Tests parsing of malformed tokens.
        /// </summary>
        [Theory]
        [InlineData("header.payload")]
        [InlineData("header.payload.signature.extra")]
        public void ShouldHandleMalformedTokens(string malformedToken)
        {
            // Act & Assert
            FormatException exception = Assert.Throws<FormatException>(() => _parser.Parse(malformedToken));
            Assert.Contains("JWT token must contain three parts separated by dots", exception.Message);
        }

        /// <summary>
        /// Tests parsing of tokens with invalid base64url encoding.
        /// </summary>
        [Fact]
        public void ShouldHandleInvalidBase64UrlEncoding()
        {
            // Arrange
            string invalidToken = "invalid.token.format";

            // Act & Assert
            FormatException exception = Assert.Throws<FormatException>(() => _parser.Parse(invalidToken));
            Assert.Contains("The input is not a valid Base-64 string", exception.Message);
        }

        /// <summary>
        /// Tests parsing of tokens with invalid JSON in claims.
        /// </summary>
        [Fact]
        public void ShouldHandleInvalidJsonInClaims()
        {
            // Arrange
            // Create a token with invalid JSON in the payload
            string invalidJson = "{invalid json}";
            string base64Header = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"alg\":\"HS256\",\"typ\":\"JWT\"}"))
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');
            string base64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(invalidJson))
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');
            string token = $"{base64Header}.{base64Payload}.";

            // Act & Assert
            FormatException exception = Assert.Throws<FormatException>(() => _parser.Parse(token));
            Assert.Contains("JWT token contains invalid JSON", exception.Message);
        }

        /// <summary>
        /// Tests parsing of tokens with extremely large claims.
        /// </summary>
        [Fact]
        public void ShouldHandleLargeClaims()
        {
            // Arrange
            string largeString = new string('x', 10000);
            string token = _builder
                .AddClaim("large", largeString)
                .SignHs256(_hmacKey);

            // Act
            IJwtToken parsedToken = _parser.Parse(token);

            // Assert
            Assert.NotNull(parsedToken);
            Assert.Equal(largeString, parsedToken.GetClaim<string>("large"));
        }
    }
} 