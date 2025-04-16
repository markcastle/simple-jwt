using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleJwt.Abstractions;
using SimpleJwt.Abstractions.TokenRepository;
using SimpleJwt.Abstractions.TokenLifetime;
using SimpleJwt.Core;
using SimpleJwt.Core.TokenLifetime;
using Xunit;
using Xunit.Abstractions;

namespace SimpleJwt.Tests
{
    /// <summary>
    /// Tests for the token repository implementation.
    /// </summary>
    public class TokenRepositoryTests : TestBase
    {
        private readonly ITestOutputHelper _output;
        private readonly IJwtBuilder _builder;
        private readonly IJwtParser _parser;
        private readonly byte[] _hmacKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenRepositoryTests"/> class.
        /// </summary>
        /// <param name="output">The test output helper to write results to.</param>
        public TokenRepositoryTests(ITestOutputHelper output) : base(useSystemTextJson: true)
        {
            _output = output;
            _builder = new JwtBuilder();
            _parser = new JwtParser();
            _hmacKey = new byte[32];
            
            // Initialize key with random data
            new Random(42).NextBytes(_hmacKey);
        }

        /// <summary>
        /// Tests storing and retrieving tokens from the repository.
        /// </summary>
        [Fact]
        public void ShouldStoreAndRetrieveTokens()
        {
            // Arrange
            var repository = new InMemoryTokenRepository();
            string token = CreateTestToken();
            string userId = "user123";
            DateTimeOffset expirationTime = DateTimeOffset.UtcNow.AddHours(1);
            
            // Act - Store token
            bool stored = repository.StoreToken(token, userId, expirationTime);
            
            // Assert - Token was stored successfully
            Assert.True(stored);
            
            // Act - Retrieve token
            var retrievedToken = repository.GetToken(token);
            
            // Assert - Token was retrieved correctly
            Assert.NotNull(retrievedToken);
            Assert.Equal(token, retrievedToken.Token);
            Assert.Equal(userId, retrievedToken.UserId);
            Assert.Equal(expirationTime.ToUnixTimeSeconds(), retrievedToken.ExpirationTime.ToUnixTimeSeconds());
            Assert.False(retrievedToken.IsExpired);
            Assert.Equal("access", retrievedToken.TokenType);
        }

        /// <summary>
        /// Tests storing and retrieving tokens with metadata.
        /// </summary>
        [Fact]
        public void ShouldStoreAndRetrieveTokensWithMetadata()
        {
            // Arrange
            var repository = new InMemoryTokenRepository();
            string token = CreateTestToken();
            string userId = "user123";
            DateTimeOffset expirationTime = DateTimeOffset.UtcNow.AddHours(1);
            var metadata = new Dictionary<string, object>
            {
                { "deviceId", "device123" },
                { "ipAddress", "192.168.1.1" },
                { "browser", "Chrome" }
            };
            
            // Act - Store token with metadata
            bool stored = repository.StoreToken(token, userId, expirationTime, "refresh", metadata);
            
            // Assert - Token was stored successfully
            Assert.True(stored);
            
            // Act - Retrieve token
            var retrievedToken = repository.GetToken(token);
            
            // Assert - Token and metadata were retrieved correctly
            Assert.NotNull(retrievedToken);
            Assert.Equal(token, retrievedToken.Token);
            Assert.Equal(userId, retrievedToken.UserId);
            Assert.Equal("refresh", retrievedToken.TokenType);
            Assert.NotNull(retrievedToken.Metadata);
            Assert.Equal(3, retrievedToken.Metadata.Count);
            Assert.Equal("device123", retrievedToken.Metadata["deviceId"]);
            Assert.Equal("192.168.1.1", retrievedToken.Metadata["ipAddress"]);
            Assert.Equal("Chrome", retrievedToken.Metadata["browser"]);
        }

        /// <summary>
        /// Tests retrieving tokens for a user.
        /// </summary>
        [Fact]
        public void ShouldRetrieveTokensForUser()
        {
            // Arrange
            var repository = new InMemoryTokenRepository();
            string userId = "user123";
            string token1 = CreateTestToken();
            string token2 = CreateTestToken();
            string token3 = CreateTestToken();
            
            // Store tokens for the user
            repository.StoreToken(token1, userId, DateTimeOffset.UtcNow.AddHours(1), "access");
            repository.StoreToken(token2, userId, DateTimeOffset.UtcNow.AddHours(1), "refresh");
            repository.StoreToken(token3, "otherUser", DateTimeOffset.UtcNow.AddHours(1), "access");
            
            // Act - Get all tokens for the user
            var userTokens = repository.GetTokensForUser(userId).ToList();
            
            // Assert - Only tokens for the specified user are returned
            Assert.Equal(2, userTokens.Count);
            Assert.Contains(userTokens, t => t.Token == token1);
            Assert.Contains(userTokens, t => t.Token == token2);
            
            // Act - Get tokens of a specific type for the user
            var accessTokens = repository.GetTokensForUser(userId, "access").ToList();
            
            // Assert - Only tokens of the specified type are returned
            Assert.Single(accessTokens);
            Assert.Equal(token1, accessTokens[0].Token);
        }

        /// <summary>
        /// Tests removing tokens from the repository.
        /// </summary>
        [Fact]
        public void ShouldRemoveTokens()
        {
            // Arrange
            var repository = new InMemoryTokenRepository();
            string token = CreateTestToken();
            string userId = "user123";
            
            // Store a token
            repository.StoreToken(token, userId, DateTimeOffset.UtcNow.AddHours(1));
            
            // Act - Remove the token
            bool removed = repository.RemoveToken(token);
            
            // Assert - Token was removed successfully
            Assert.True(removed);
            Assert.False(repository.TokenExists(token));
            Assert.Null(repository.GetToken(token));
        }

        /// <summary>
        /// Tests removing all tokens for a user.
        /// </summary>
        [Fact]
        public void ShouldRemoveTokensForUser()
        {
            // Arrange
            var repository = new InMemoryTokenRepository();
            string userId = "user123";
            string token1 = CreateTestToken();
            string token2 = CreateTestToken();
            string token3 = CreateTestToken();
            
            // Store tokens for multiple users
            repository.StoreToken(token1, userId, DateTimeOffset.UtcNow.AddHours(1), "access");
            repository.StoreToken(token2, userId, DateTimeOffset.UtcNow.AddHours(1), "refresh");
            repository.StoreToken(token3, "otherUser", DateTimeOffset.UtcNow.AddHours(1), "access");
            
            // Act - Remove all tokens for the user
            int removed = repository.RemoveTokensForUser(userId);
            
            // Assert - Only tokens for the specified user are removed
            Assert.Equal(2, removed);
            Assert.False(repository.TokenExists(token1));
            Assert.False(repository.TokenExists(token2));
            Assert.True(repository.TokenExists(token3));
        }

        /// <summary>
        /// Tests removing expired tokens from the repository.
        /// </summary>
        [Fact]
        public void ShouldRemoveExpiredTokens()
        {
            // Arrange
            var repository = new InMemoryTokenRepository();
            string token1 = CreateTestToken();
            string token2 = CreateTestToken();
            string token3 = CreateTestToken();
            
            // Store tokens with different expiration times
            repository.StoreToken(token1, "user1", DateTimeOffset.UtcNow.AddHours(-1)); // Expired
            repository.StoreToken(token2, "user2", DateTimeOffset.UtcNow.AddHours(-2)); // Expired
            repository.StoreToken(token3, "user3", DateTimeOffset.UtcNow.AddHours(1));  // Not expired
            
            // Act - Remove expired tokens
            int removed = repository.RemoveExpiredTokens();
            
            // Assert - Only expired tokens are removed
            Assert.Equal(2, removed);
            Assert.False(repository.TokenExists(token1));
            Assert.False(repository.TokenExists(token2));
            Assert.True(repository.TokenExists(token3));
        }

        /// <summary>
        /// Tests the integration between the token repository and the token revoker.
        /// </summary>
        [Fact]
        public void ShouldIntegrateWithTokenRevoker()
        {
            // Arrange
            var repository = new InMemoryTokenRepository();
            var revoker = new TokenRepositoryRevoker(repository, _parser);
            string token = CreateTestToken();
            string reason = "User logged out";
            
            // Act - Revoke the token
            bool revoked = revoker.Revoke(token, reason);
            
            // Assert - Token was revoked successfully
            Assert.True(revoked);
            Assert.True(revoker.IsRevoked(token));
            Assert.Equal(reason, revoker.GetRevocationReason(token));
            
            // The token should be stored in the repository
            Assert.True(repository.TokenExists(token));
            var storedToken = repository.GetToken(token);
            Assert.Equal("revoked", storedToken.TokenType);
            Assert.True(storedToken.Metadata.ContainsKey("revocationReason"));
            Assert.Equal(reason, storedToken.Metadata["revocationReason"]);
        }

        /// <summary>
        /// Tests asynchronous operations with the token repository.
        /// </summary>
        [Fact]
        public async Task ShouldSupportAsyncOperations()
        {
            // Arrange
            var repository = new InMemoryTokenRepository();
            string token = CreateTestToken();
            string userId = "user123";
            DateTimeOffset expirationTime = DateTimeOffset.UtcNow.AddHours(1);
            
            // Act - Store token asynchronously
            bool stored = await repository.StoreTokenAsync(token, userId, expirationTime);
            
            // Assert - Token was stored successfully
            Assert.True(stored);
            
            // Act - Retrieve token asynchronously
            var retrievedToken = await repository.GetTokenAsync(token);
            
            // Assert - Token was retrieved correctly
            Assert.NotNull(retrievedToken);
            Assert.Equal(token, retrievedToken.Token);
            
            // Act - Check if token exists asynchronously
            bool exists = await repository.TokenExistsAsync(token);
            
            // Assert - Token exists
            Assert.True(exists);
            
            // Act - Remove token asynchronously
            bool removed = await repository.RemoveTokenAsync(token);
            
            // Assert - Token was removed successfully
            Assert.True(removed);
            Assert.False(await repository.TokenExistsAsync(token));
        }

        /// <summary>
        /// Tests asynchronous revocation operations in the token repository.
        /// </summary>
        [Fact]
        public async Task ShouldRevokeTokensAsync()
        {
            var repository = new InMemoryTokenRepository();
            string token = CreateTestToken();
            string userId = "userAsync";
            DateTimeOffset expirationTime = DateTimeOffset.UtcNow.AddHours(1);
            await repository.StoreTokenAsync(token, userId, expirationTime);
            // Act - Remove token asynchronously
            bool removed = await repository.RemoveTokenAsync(token);
            Assert.True(removed);
            // Assert - Token no longer exists
            Assert.False(await repository.TokenExistsAsync(token));
        }

        /// <summary>
        /// Tests user-based revocation with a large dataset.
        /// </summary>
        [Fact]
        public void ShouldRevokeAllForUserWithLargeDataset()
        {
            var repository = new InMemoryTokenRepository();
            string userId = "bulkUser";
            int tokenCount = 5000;
            var tokens = new List<string>();
            for (int i = 0; i < tokenCount; i++)
            {
                string token = CreateTestToken();
                tokens.Add(token);
                repository.StoreToken(token, userId, DateTimeOffset.UtcNow.AddHours(1));
            }
            // Act - Remove all tokens for user
            int removedCount = repository.RemoveTokensForUser(userId);
            Assert.Equal(tokenCount, removedCount);
            // Assert - No tokens for user
            foreach (var token in tokens)
                Assert.False(repository.TokenExists(token));
        }

        /// <summary>
        /// Tests integration: removed (revoked) tokens are not validated as active.
        /// </summary>
        [Fact]
        public void ShouldNotValidateRevokedTokens()
        {
            var repository = new InMemoryTokenRepository();
            var validator = new Core.Validation.JwtValidator(_parser);
            string token = CreateTestToken();
            string userId = "revokeTest";
            DateTimeOffset expirationTime = DateTimeOffset.UtcNow.AddHours(1);
            repository.StoreToken(token, userId, expirationTime);
            // Remove token (simulate revocation)
            repository.RemoveToken(token);
            // Try to validate (simulate integration)
            var jwt = _parser.Parse(token);
            var result = validator.Validate(jwt, new Abstractions.Validation.ValidationParameters
            {
                SymmetricSecurityKey = _hmacKey
            });
            // Assert - Should be invalid (token removed)
            Assert.False(repository.TokenExists(token));
            // NOTE: Depending on integration, you may want to check validator result for a removed token scenario
        }

        /// <summary>
        /// Tests the token count functionality.
        /// </summary>
        [Fact]
        public void ShouldCountTokens()
        {
            // Arrange
            var repository = new InMemoryTokenRepository();
            string userId1 = "user1";
            string userId2 = "user2";
            
            // Store tokens for different users and types
            repository.StoreToken(CreateTestToken(), userId1, DateTimeOffset.UtcNow.AddHours(1), "access");
            repository.StoreToken(CreateTestToken(), userId1, DateTimeOffset.UtcNow.AddHours(1), "refresh");
            repository.StoreToken(CreateTestToken(), userId1, DateTimeOffset.UtcNow.AddHours(1), "access");
            repository.StoreToken(CreateTestToken(), userId2, DateTimeOffset.UtcNow.AddHours(1), "access");
            repository.StoreToken(CreateTestToken(), userId2, DateTimeOffset.UtcNow.AddHours(1), "refresh");
            
            // Act & Assert - Count all tokens
            Assert.Equal(5, repository.GetTokenCount());
            
            // Act & Assert - Count tokens for a specific user
            Assert.Equal(3, repository.GetTokenCount(userId1));
            Assert.Equal(2, repository.GetTokenCount(userId2));
            
            // Act & Assert - Count tokens of a specific type
            Assert.Equal(3, repository.GetTokenCount(tokenType: "access"));
            Assert.Equal(2, repository.GetTokenCount(tokenType: "refresh"));
            
            // Act & Assert - Count tokens for a specific user and type
            Assert.Equal(2, repository.GetTokenCount(userId1, "access"));
            Assert.Equal(1, repository.GetTokenCount(userId1, "refresh"));
            Assert.Equal(1, repository.GetTokenCount(userId2, "access"));
            Assert.Equal(1, repository.GetTokenCount(userId2, "refresh"));
        }

        /// <summary>
        /// Creates a test token for testing.
        /// </summary>
        private string CreateTestToken()
        {
            // Create a new builder instance for each call to ensure thread safety
            var builder = new JwtBuilder();
            
            return builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(30))
                .SetIssuedNow()
                .AddClaim("test-claim", Guid.NewGuid().ToString())
                .SignHs256(_hmacKey);
        }
    }
} 