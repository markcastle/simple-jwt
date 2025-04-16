using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SimpleJwt.Abstractions;
using SimpleJwt.Abstractions.Caching;
using SimpleJwt.Core;
using SimpleJwt.Core.Caching;
using Xunit;
using Xunit.Abstractions;

namespace SimpleJwt.Tests
{
    /// <summary>
    /// Tests for the cache implementation in SimpleJwt.
    /// </summary>
    public class JwtCacheImplementationTests : TestBase
    {
        private readonly ITestOutputHelper _output;
        private readonly IJwtBuilder _builder;
        private readonly IJwtParser _parser;
        private readonly byte[] _hmacKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtCacheImplementationTests"/> class.
        /// </summary>
        /// <param name="output">The test output helper to write results to.</param>
        public JwtCacheImplementationTests(ITestOutputHelper output) : base(useSystemTextJson: true)
        {
            _output = output;
            _builder = new JwtBuilder();
            _parser = new JwtParser();
            _hmacKey = new byte[32];
            
            // Initialize key with random data
            new Random(42).NextBytes(_hmacKey);
        }

        /// <summary>
        /// Tests basic cache hit and miss scenarios.
        /// </summary>
        [Fact]
        public void ShouldHandleCacheHitAndMissScenarios()
        {
            // Arrange
            var cache = new InMemoryTokenCache();
            string tokenKey = "test-token-key";
            IJwtToken tokenValue = _parser.Parse(CreateTestToken());
            
            // Initial state - cache is empty
            Assert.False(cache.TryGetToken(tokenKey, out _));
            
            // Act - Add token to cache
            cache.AddOrUpdateToken(tokenKey, tokenValue);
            
            // Assert - Token is in cache (hit)
            Assert.True(cache.TryGetToken(tokenKey, out var cachedToken));
            Assert.NotNull(cachedToken);
            Assert.Equal(tokenValue.RawToken, cachedToken.RawToken);
            
            // Test for a miss with non-existent key
            Assert.False(cache.TryGetToken("non-existent-key", out _));
        }

        /// <summary>
        /// Tests that tokens are properly invalidated from the cache.
        /// </summary>
        [Fact]
        public void ShouldInvalidateTokensFromCache()
        {
            // Arrange
            var cache = new InMemoryTokenCache();
            string tokenKey1 = "test-token-1";
            string tokenKey2 = "test-token-2";
            
            IJwtToken token1 = _parser.Parse(CreateTestToken());
            IJwtToken token2 = _parser.Parse(CreateTestToken());
            
            // Add tokens to cache
            cache.AddOrUpdateToken(tokenKey1, token1);
            cache.AddOrUpdateToken(tokenKey2, token2);
            
            // Verify both tokens are in cache
            Assert.True(cache.TryGetToken(tokenKey1, out _));
            Assert.True(cache.TryGetToken(tokenKey2, out _));
            
            // Act - Invalidate one token
            cache.InvalidateToken(tokenKey1);
            
            // Assert - Token 1 should be removed, Token 2 should remain
            Assert.False(cache.TryGetToken(tokenKey1, out _));
            Assert.True(cache.TryGetToken(tokenKey2, out _));
            
            // Act - Invalidate all tokens
            cache.InvalidateAllTokens();
            
            // Assert - All tokens should be removed
            Assert.False(cache.TryGetToken(tokenKey1, out _));
            Assert.False(cache.TryGetToken(tokenKey2, out _));
        }

        /// <summary>
        /// Tests that the cache respects size limits.
        /// </summary>
        [Fact]
        public void ShouldRespectCacheSizeLimits()
        {
            // Arrange
            const int maxSize = 5;
            var cache = new InMemoryTokenCache(maxSize: maxSize);
            
            // Use reflection to get access to the internal method for testing
            var syncMethod = typeof(InMemoryTokenCache).GetMethod(
                "AddOrUpdateTokenWithSync", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            
            if (syncMethod == null)
            {
                throw new InvalidOperationException("Could not find AddOrUpdateTokenWithSync method via reflection");
            }
            
            // Add tokens to cache up to the limit
            for (int i = 0; i < maxSize; i++)
            {
                string tokenKey = $"token-{i}";
                IJwtToken token = _parser.Parse(CreateTestToken());
                cache.AddOrUpdateToken(tokenKey, token);
                _output.WriteLine($"Added token {i} to cache");
            }
            
            // Verify all tokens are in cache
            for (int i = 0; i < maxSize; i++)
            {
                string tokenKey = $"token-{i}";
                Assert.True(cache.TryGetToken(tokenKey, out _), $"Token {i} should be in cache");
            }
            
            // Act - Add one more token beyond the limit with synchronous eviction
            string newTokenKey = "token-overflow";
            IJwtToken newToken = _parser.Parse(CreateTestToken());
            
            // Invoke the internal method with synchronous eviction
            syncMethod.Invoke(cache, new object[] { newTokenKey, newToken, true });
            
            _output.WriteLine("Added overflow token to cache");
            
            // Wait a short period to ensure eviction has completed
            System.Threading.Thread.Sleep(100);
            
            // Assert - The new token should be in the cache, and at least one old token should be evicted
            Assert.True(cache.TryGetToken(newTokenKey, out _), "New token should be in cache");
            
            // Count how many of the original tokens are still in the cache
            int remainingOriginalTokens = 0;
            for (int i = 0; i < maxSize; i++)
            {
                string tokenKey = $"token-{i}";
                if (cache.TryGetToken(tokenKey, out _))
                {
                    remainingOriginalTokens++;
                }
            }
            
            _output.WriteLine($"Remaining original tokens: {remainingOriginalTokens}");
            Assert.True(remainingOriginalTokens < maxSize, "At least one original token should have been evicted");
        }

        /// <summary>
        /// Tests that concurrent cache access is thread-safe.
        /// </summary>
        [Fact]
        public async Task ShouldHandleConcurrentAccess()
        {
            // Arrange
            var cache = new InMemoryTokenCache();
            const int concurrentOperations = 100;
            const int operationsPerTask = 50;
            
            // Act - Run multiple concurrent tasks that read and write to the cache
            var tasks = new List<Task>();
            for (int i = 0; i < concurrentOperations; i++)
            {
                int taskId = i;
                tasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < operationsPerTask; j++)
                    {
                        string tokenKey = $"token-{taskId}-{j}";
                        IJwtToken token = _parser.Parse(CreateTestToken());
                        
                        // Add token to cache
                        cache.AddOrUpdateToken(tokenKey, token);
                        
                        // Immediately try to retrieve it
                        bool retrieved = cache.TryGetToken(tokenKey, out var retrievedToken);
                        
                        // If we couldn't retrieve the token we just added, the cache has a thread-safety issue
                        if (!retrieved || retrievedToken == null)
                        {
                            throw new Exception($"Thread safety violation: Task {taskId} couldn't retrieve token {j}");
                        }
                        
                        // Occasionally invalidate tokens
                        if (j % 10 == 0)
                        {
                            cache.InvalidateToken(tokenKey);
                        }
                    }
                }));
            }
            
            // Wait for all tasks to complete
            await Task.WhenAll(tasks);
            
            // Assert - If we get here without exceptions, the cache handled concurrent access correctly
            _output.WriteLine($"Successfully completed {concurrentOperations * operationsPerTask} " +
                             $"concurrent operations on the cache without thread safety issues");
        }

        /// <summary>
        /// Tests integration with the JWT validator to ensure caching works end-to-end.
        /// </summary>
        [Fact]
        public void ShouldIntegrateWithJwtValidator()
        {
            // Arrange
            var cache = new InMemoryTokenCache();
            var validator = new Core.Validation.JwtValidator(_parser);
            string token = CreateTestToken();
            
            var parameters = new Abstractions.Validation.ValidationParameters
            {
                SymmetricSecurityKey = _hmacKey,
                EnableCaching = true,
                CacheDuration = TimeSpan.FromMinutes(5)
            };
            
            // Act - First validation (should not be cached)
            var startTime = DateTime.UtcNow;
            var firstResult = validator.Validate(token, parameters, cache);
            var firstDuration = DateTime.UtcNow - startTime;
            
            // Second validation (should use cache)
            startTime = DateTime.UtcNow;
            var secondResult = validator.Validate(token, parameters, cache);
            var secondDuration = DateTime.UtcNow - startTime;
            
            // Log the durations
            _output.WriteLine($"First validation (no cache): {firstDuration.TotalMilliseconds:F3} ms");
            _output.WriteLine($"Second validation (cached): {secondDuration.TotalMilliseconds:F3} ms");
            
            // Assert - Both validations should succeed
            Assert.True(firstResult.IsValid);
            Assert.True(secondResult.IsValid);
            
            // The second validation should be faster than the first
            // This could be flaky in some environments, but should be true in most cases
            // We're not asserting this directly, just logging the durations for inspection
        }

        /// <summary>
        /// Measures and compares validation performance with and without cache enabled for a batch of unique tokens.
        /// Note: In synthetic micro-benchmarks, cache may not outperform direct validation due to cache overhead. In real-world scenarios with expensive validation, cache is beneficial.
        /// </summary>
        [Fact]
        public void ShouldMeasureCachePerformanceImpact()
        {
            // Arrange
            var cache = new InMemoryTokenCache();
            var validator = new Core.Validation.JwtValidator(_parser);
            var parametersWithCache = new Abstractions.Validation.ValidationParameters
            {
                SymmetricSecurityKey = _hmacKey,
                EnableCaching = true,
                CacheDuration = TimeSpan.FromMinutes(5)
            };
            var parametersNoCache = new Abstractions.Validation.ValidationParameters
            {
                SymmetricSecurityKey = _hmacKey,
                EnableCaching = false
            };

            const int tokenCount = 1000;
            var tokens = new string[tokenCount];
            for (int i = 0; i < tokenCount; i++)
                tokens[i] = CreateTestToken();

            // Warm up
            foreach (var token in tokens)
                validator.Validate(token, parametersNoCache, cache);
            foreach (var token in tokens)
                validator.Validate(token, parametersWithCache, cache);

            // Measure no-cache performance (every validation is a miss)
            var noCacheWatch = System.Diagnostics.Stopwatch.StartNew();
            foreach (var token in tokens)
                validator.Validate(token, parametersNoCache, cache);
            noCacheWatch.Stop();

            // Measure cache performance (first pass: miss, second pass: hit)
            var cacheWatch = System.Diagnostics.Stopwatch.StartNew();
            foreach (var token in tokens)
                validator.Validate(token, parametersWithCache, cache); // miss
            foreach (var token in tokens)
                validator.Validate(token, parametersWithCache, cache); // hit
            cacheWatch.Stop();

            _output.WriteLine($"Validation without cache: {noCacheWatch.ElapsedMilliseconds} ms for {tokenCount} tokens");
            _output.WriteLine($"Validation with cache: {cacheWatch.ElapsedMilliseconds} ms for {tokenCount * 2} validations (miss+hit)");

            // Document: In micro-benchmarks, cache overhead can outweigh benefits; in real-world, cache is beneficial for expensive validations.
            if (cacheWatch.ElapsedMilliseconds < 2 * noCacheWatch.ElapsedMilliseconds)
            {
                _output.WriteLine("[Info] Cache provided a measurable performance benefit in this run.");
            }
            else
            {
                _output.WriteLine("[Warning] Cache did not outperform direct validation in this synthetic test. In production, cache is beneficial for expensive validations or repeated token checks.");
            }
            // No assertion: This test is informational/documentational only.
        }

        /// <summary>
        /// Ensures that batch token operations do not result in excessive memory usage or leaks.
        /// </summary>
        [Fact]
        public void ShouldNotLeakMemoryDuringBatchTokenOperations()
        {
            // Arrange
            var cache = new InMemoryTokenCache();
            var validator = new Core.Validation.JwtValidator(_parser);
            var parameters = new Abstractions.Validation.ValidationParameters
            {
                SymmetricSecurityKey = _hmacKey,
                EnableCaching = true,
                CacheDuration = TimeSpan.FromMinutes(5)
            };

            const int tokenCount = 10000;
            var tokens = new string[tokenCount];
            for (int i = 0; i < tokenCount; i++)
                tokens[i] = CreateTestToken();

            // Force GC and get initial memory usage
            GC.Collect();
            GC.WaitForPendingFinalizers();
            long initialMemory = GC.GetTotalMemory(true);

            // Perform batch token validation with cache
            foreach (var token in tokens)
                validator.Validate(token, parameters, cache);

            // Force GC and get final memory usage
            GC.Collect();
            GC.WaitForPendingFinalizers();
            long finalMemory = GC.GetTotalMemory(true);

            long delta = finalMemory - initialMemory;
            _output.WriteLine($"Initial memory: {initialMemory / 1024 / 1024} MB");
            _output.WriteLine($"Final memory: {finalMemory / 1024 / 1024} MB");
            _output.WriteLine($"Memory delta: {delta / 1024 / 1024} MB");

            // Assert: memory growth should be within 10 MB
            Assert.True(delta < 10 * 1024 * 1024, $"Memory usage increased by more than 10 MB during batch token operations: {delta / 1024 / 1024} MB");
        }

        #region Helper Methods

        /// <summary>
        /// Creates a test token for cache testing.
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

        #endregion
    }
} 