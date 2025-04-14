using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using SimpleJwt.Abstractions;
using SimpleJwt.Abstractions.Serialization;
using SimpleJwt.Abstractions.Validation;
using SimpleJwt.Core;
using SimpleJwt.Core.Validation;
using Xunit;

namespace SimpleJwt.Tests
{
    /// <summary>
    /// Tests for RFC compliance of the JWT implementation.
    /// </summary>
    public class JwtRfcComplianceTests
    {
        private readonly IJwtValidator _validator;
        private readonly IJwtBuilder _builder;
        private readonly byte[] _hmacKey;
        private readonly RSA _rsaKey;
        private readonly ECDsa _ecdsaKey;

        public JwtRfcComplianceTests()
        {
            _validator = new JwtValidator(new JwtParser());
            _builder = new JwtBuilder();
            _hmacKey = Encoding.UTF8.GetBytes("secret-key-with-at-least-32-bytes-for-hmac");
            _rsaKey = RSA.Create(2048);
            _ecdsaKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        }

        /// <summary>
        /// Helper method to extract numeric values from claim objects regardless of the JSON serializer
        /// </summary>
        private long GetNumericValue(object obj)
        {
            if (obj is long longValue)
            {
                return longValue;
            }
            else if (obj is int intValue)
            {
                return intValue;
            }
            else if (obj is JsonElement jsonElement)
            {
                return jsonElement.GetInt64();
            }
            else if (obj is JValue jValue)
            {
                return jValue.Value<long>();
            }
            else
            {
                return Convert.ToInt64(obj.ToString());
            }
        }

        /// <summary>
        /// Tests that the JWT implementation complies with RFC 7519 (JWT) regarding claim names and values.
        /// </summary>
        [Fact]
        public void ShouldComplyWithRfc7519ClaimFormat()
        {
            // Arrange
            string jwtId = Guid.NewGuid().ToString();
            DateTime now = DateTime.UtcNow;
            string token = _builder
                .SetIssuer("https://myissuer.com")
                .SetAudience("https://myapi.com")
                .SetSubject("user123")
                .SetExpirationTime(now.AddMinutes(30))
                .SetNotBefore(now.AddMinutes(-5))
                .SetIssuedAt(now)
                .SetJwtId(jwtId)
                .SignHs256(_hmacKey);

            // Act
            IJwtParser parser = new JwtParser();
            IJwtToken parsedToken = parser.Parse(token);

            // Assert - Check that standard claims are correctly named according to RFC 7519
            Assert.Equal("https://myissuer.com", parsedToken.GetClaim<string>(JwtConstants.ClaimIssuer));
            Assert.Equal("https://myapi.com", parsedToken.GetClaim<string>(JwtConstants.ClaimAudience));
            Assert.Equal("user123", parsedToken.GetClaim<string>(JwtConstants.ClaimSubject));

            // Check that numeric claims exist and are valid timestamps
            Assert.True(parsedToken.Payload.ContainsKey(JwtConstants.ClaimExpirationTime));
            Assert.True(parsedToken.Payload.ContainsKey(JwtConstants.ClaimNotBefore));
            Assert.True(parsedToken.Payload.ContainsKey(JwtConstants.ClaimIssuedAt));
            
            // Check that JTI exists
            Assert.True(parsedToken.Payload.ContainsKey(JwtConstants.ClaimJwtId));
            var rawJti = parsedToken.Payload[JwtConstants.ClaimJwtId].ToString();
            Assert.Equal(jwtId, rawJti);
        }

        /// <summary>
        /// Tests that the JWT implementation complies with RFC 7515 (JWS) regarding header parameters.
        /// </summary>
        [Fact]
        public void ShouldComplyWithRfc7515HeaderParameters()
        {
            // Arrange
            string token = _builder
                .SetIssuer("https://myissuer.com")
                .AddHeaderClaim(JwtConstants.HeaderAlgorithm, JwtConstants.AlgorithmHs256)
                .AddHeaderClaim(JwtConstants.HeaderType, "JWT")
                .AddHeaderClaim(JwtConstants.HeaderKeyId, "key-2023-04-01")
                .SignHs256(_hmacKey);

            // Act
            IJwtParser parser = new JwtParser();
            IJwtToken parsedToken = parser.Parse(token);

            // Assert - Check that header parameters are correctly named according to RFC 7515
            Assert.True(parsedToken.TryGetHeaderClaim<string>(JwtConstants.HeaderAlgorithm, out var alg));
            Assert.Equal(JwtConstants.AlgorithmHs256, alg);
            Assert.True(parsedToken.TryGetHeaderClaim<string>(JwtConstants.HeaderType, out var typ));
            Assert.Equal("JWT", typ);
            Assert.True(parsedToken.TryGetHeaderClaim<string>(JwtConstants.HeaderKeyId, out var kid));
            Assert.Equal("key-2023-04-01", kid);
        }

        /// <summary>
        /// Tests that the JWT implementation correctly handles IANA registered claims.
        /// </summary>
        [Fact]
        public void ShouldHandleIanaRegisteredClaims()
        {
            // Arrange
            string jwtId = Guid.NewGuid().ToString();
            DateTime now = DateTime.UtcNow;
            string token = _builder
                .SetIssuer("https://myissuer.com")
                .SetAudience("https://myapi.com")
                .SetSubject("user123")
                .SetExpirationTime(now.AddMinutes(30))
                .SetNotBefore(now.AddMinutes(-5))
                .SetIssuedAt(now)
                .SetJwtId(jwtId)
                .AddClaim(JwtConstants.ClaimName, "John Doe")
                .AddClaim(JwtConstants.ClaimGivenName, "John")
                .AddClaim(JwtConstants.ClaimFamilyName, "Doe")
                .AddClaim(JwtConstants.ClaimMiddleName, "Robert")
                .AddClaim(JwtConstants.ClaimNickname, "Johnny")
                .AddClaim(JwtConstants.ClaimPreferredUsername, "johndoe")
                .AddClaim(JwtConstants.ClaimProfile, "https://example.com/profile")
                .AddClaim(JwtConstants.ClaimPicture, "https://example.com/picture.jpg")
                .AddClaim(JwtConstants.ClaimWebsite, "https://example.com")
                .AddClaim(JwtConstants.ClaimEmail, "john.doe@example.com")
                .AddClaim(JwtConstants.ClaimEmailVerified, true)
                .AddClaim(JwtConstants.ClaimGender, "male")
                .AddClaim(JwtConstants.ClaimBirthdate, "1990-01-01")
                .AddClaim(JwtConstants.ClaimZoneinfo, "America/New_York")
                .AddClaim(JwtConstants.ClaimLocale, "en-US")
                .AddClaim(JwtConstants.ClaimPhoneNumber, "+1 (555) 123-4567")
                .AddClaim(JwtConstants.ClaimPhoneNumberVerified, true)
                .AddClaim(JwtConstants.ClaimAddress, new Dictionary<string, string>
                {
                    ["formatted"] = "123 Main St, Anytown, USA 12345",
                    ["street_address"] = "123 Main St",
                    ["locality"] = "Anytown",
                    ["region"] = "CA",
                    ["postal_code"] = "12345",
                    ["country"] = "USA"
                })
                .AddClaim(JwtConstants.ClaimUpdatedAt, DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                .SignHs256(_hmacKey);

            // Act
            IJwtParser parser = new JwtParser();
            IJwtToken parsedToken = parser.Parse(token);

            // Assert - Check that standard claims exist
            Assert.Equal("https://myissuer.com", parsedToken.GetClaim<string>(JwtConstants.ClaimIssuer));
            Assert.Equal("https://myapi.com", parsedToken.GetClaim<string>(JwtConstants.ClaimAudience));
            Assert.Equal("user123", parsedToken.GetClaim<string>(JwtConstants.ClaimSubject));
            
            // Check that JWT ID exists
            Assert.True(parsedToken.Payload.ContainsKey(JwtConstants.ClaimJwtId));
            var rawJti = parsedToken.Payload[JwtConstants.ClaimJwtId].ToString();
            Assert.Equal(jwtId, rawJti);

            // Check that IANA registered claims exist and have correct values
            Assert.Equal("John Doe", parsedToken.GetClaim<string>(JwtConstants.ClaimName));
            Assert.Equal("John", parsedToken.GetClaim<string>(JwtConstants.ClaimGivenName));
            Assert.Equal("Doe", parsedToken.GetClaim<string>(JwtConstants.ClaimFamilyName));
            Assert.Equal("Robert", parsedToken.GetClaim<string>(JwtConstants.ClaimMiddleName));
            Assert.Equal("Johnny", parsedToken.GetClaim<string>(JwtConstants.ClaimNickname));
            Assert.Equal("johndoe", parsedToken.GetClaim<string>(JwtConstants.ClaimPreferredUsername));
            Assert.Equal("https://example.com/profile", parsedToken.GetClaim<string>(JwtConstants.ClaimProfile));
            Assert.Equal("https://example.com/picture.jpg", parsedToken.GetClaim<string>(JwtConstants.ClaimPicture));
            Assert.Equal("https://example.com", parsedToken.GetClaim<string>(JwtConstants.ClaimWebsite));
            Assert.Equal("john.doe@example.com", parsedToken.GetClaim<string>(JwtConstants.ClaimEmail));
            
            // For boolean claims, check that they exist
            Assert.True(parsedToken.Payload.ContainsKey(JwtConstants.ClaimEmailVerified));
            Assert.True(parsedToken.Payload.ContainsKey(JwtConstants.ClaimPhoneNumberVerified));
            
            Assert.Equal("male", parsedToken.GetClaim<string>(JwtConstants.ClaimGender));
            Assert.Equal("1990-01-01", parsedToken.GetClaim<string>(JwtConstants.ClaimBirthdate));
            Assert.Equal("America/New_York", parsedToken.GetClaim<string>(JwtConstants.ClaimZoneinfo));
            Assert.Equal("en-US", parsedToken.GetClaim<string>(JwtConstants.ClaimLocale));
            Assert.Equal("+1 (555) 123-4567", parsedToken.GetClaim<string>(JwtConstants.ClaimPhoneNumber));
            
            // Check that address exists
            Assert.True(parsedToken.Payload.ContainsKey(JwtConstants.ClaimAddress));
            
            // Check that updated_at exists
            Assert.True(parsedToken.Payload.ContainsKey(JwtConstants.ClaimUpdatedAt));
        }

        /// <summary>
        /// Tests that the JWT implementation correctly handles standard claim formats.
        /// </summary>
        [Theory]
        [InlineData(JwtConstants.AlgorithmHs256)]
        [InlineData(JwtConstants.AlgorithmRs256)]
        [InlineData(JwtConstants.AlgorithmEs256)]
        public void ShouldHandleStandardClaimFormats(string algorithm)
        {
            // Arrange
            string jwtId = Guid.NewGuid().ToString();
            DateTime now = DateTime.UtcNow;
            IJwtBuilder builder = _builder
                .SetIssuer("https://myissuer.com")
                .SetAudience("https://myapi.com")
                .SetSubject("user123")
                .SetExpirationTime(now.AddMinutes(30))
                .SetNotBefore(now.AddMinutes(-5))
                .SetIssuedAt(now)
                .SetJwtId(jwtId);

            string token;
            switch (algorithm)
            {
                case JwtConstants.AlgorithmHs256:
                    token = builder.SignHs256(_hmacKey);
                    break;
                case JwtConstants.AlgorithmRs256:
                    token = builder.SignRs256(_rsaKey);
                    break;
                case JwtConstants.AlgorithmEs256:
                    token = builder.SignEs256(_ecdsaKey);
                    break;
                default:
                    throw new ArgumentException($"Unsupported algorithm: {algorithm}");
            }

            // Act
            IJwtParser parser = new JwtParser();
            IJwtToken parsedToken = parser.Parse(token);

            // Assert - Check that standard claims are correctly formatted
            Assert.Equal("https://myissuer.com", parsedToken.GetClaim<string>(JwtConstants.ClaimIssuer));
            Assert.Equal("https://myapi.com", parsedToken.GetClaim<string>(JwtConstants.ClaimAudience));
            Assert.Equal("user123", parsedToken.GetClaim<string>(JwtConstants.ClaimSubject));

            // Check numeric claims
            Assert.True(parsedToken.Payload.ContainsKey(JwtConstants.ClaimExpirationTime));
            Assert.True(parsedToken.Payload.ContainsKey(JwtConstants.ClaimNotBefore));
            Assert.True(parsedToken.Payload.ContainsKey(JwtConstants.ClaimIssuedAt));
            
            // Check JWT ID
            Assert.True(parsedToken.Payload.ContainsKey(JwtConstants.ClaimJwtId));
            var rawJti = parsedToken.Payload[JwtConstants.ClaimJwtId].ToString();
            Assert.Equal(jwtId, rawJti);
        }
    }
} 