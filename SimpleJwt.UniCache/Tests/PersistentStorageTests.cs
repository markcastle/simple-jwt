using System;
using System.IO;
using System.Threading.Tasks;
using Moq;
using SimpleJwt.Abstractions.Caching;
using UniCache;
using Xunit;

namespace SimpleJwt.UniCache.Tests
{
    /// <summary>
    /// Tests for persistent token storage with UniCache.
    /// </summary>
    public class PersistentStorageTests
    {
        /// <summary>
        /// Verifies tokens are persisted and retrievable across repository instances (simulated disk cache).
        /// </summary>
        [Fact]
        public async Task ShouldPersistAndRetrieveToken()
        {
            var tempFile = Path.GetTempFileName();
            var cache = new FileUniCache(tempFile);
            var repo = new UniCacheTokenRepository(cache);
            var token = Mock.Of<IJwtToken>();
            await repo.SetTokenAsync("foo", token);
            // Simulate new repository instance (e.g. after restart)
            var repo2 = new UniCacheTokenRepository(new FileUniCache(tempFile));
            var result = await repo2.GetTokenAsync("foo");
            Assert.NotNull(result);
            File.Delete(tempFile);
        }

        /// <summary>
        /// Verifies that encrypted tokens cannot be decrypted with the wrong key.
        /// </summary>
        [Fact]
        public async Task ShouldNotDecryptWithWrongKey()
        {
            var tempFile = Path.GetTempFileName();
            var key = new byte[32];
            var salt = new byte[16];
            new Random().NextBytes(key);
            new Random().NextBytes(salt);
            var settings = new CacheEncryptionSettings(key, salt);
            var cache = new FileUniCache(tempFile);
            var repo = new UniCacheTokenRepository(cache, settings);
            var token = Mock.Of<IJwtToken>();
            await repo.SetTokenAsync("foo", token);
            // Use wrong key
            var wrongKey = new byte[32];
            var wrongSalt = new byte[16];
            new Random().NextBytes(wrongKey);
            new Random().NextBytes(wrongSalt);
            var wrongSettings = new CacheEncryptionSettings(wrongKey, wrongSalt);
            var repo2 = new UniCacheTokenRepository(new FileUniCache(tempFile), wrongSettings);
            await Assert.ThrowsAsync<Exception>(() => repo2.GetTokenAsync("foo"));
            File.Delete(tempFile);
        }

        /// <summary>
        /// Ensures encrypted tokens survive simulated application restarts.
        /// </summary>
        [Fact]
        public async Task ShouldSurviveRestartsWithCorrectKey()
        {
            var tempFile = Path.GetTempFileName();
            var key = new byte[32];
            var salt = new byte[16];
            new Random().NextBytes(key);
            new Random().NextBytes(salt);
            var settings = new CacheEncryptionSettings(key, salt);
            var cache = new FileUniCache(tempFile);
            var repo = new UniCacheTokenRepository(cache, settings);
            var token = Mock.Of<IJwtToken>();
            await repo.SetTokenAsync("foo", token);
            // Simulate app restart
            var repo2 = new UniCacheTokenRepository(new FileUniCache(tempFile), settings);
            var result = await repo2.GetTokenAsync("foo");
            Assert.NotNull(result);
            File.Delete(tempFile);
        }

        /// <summary>
        /// Measures encryption performance for storing and retrieving tokens.
        /// </summary>
        [Fact]
        public async Task ShouldMeasureEncryptionPerformance()
        {
            var tempFile = Path.GetTempFileName();
            var key = new byte[32];
            var salt = new byte[16];
            new Random().NextBytes(key);
            new Random().NextBytes(salt);
            var settings = new CacheEncryptionSettings(key, salt);
            var cache = new FileUniCache(tempFile);
            var repo = new UniCacheTokenRepository(cache, settings);
            var token = Mock.Of<IJwtToken>();
            var sw = System.Diagnostics.Stopwatch.StartNew();
            await repo.SetTokenAsync("foo", token);
            var setTime = sw.ElapsedMilliseconds;
            sw.Restart();
            await repo.GetTokenAsync("foo");
            var getTime = sw.ElapsedMilliseconds;
            // Arbitrary: Assert both are < 250ms for test environments
            Assert.InRange(setTime, 0, 250);
            Assert.InRange(getTime, 0, 250);
            File.Delete(tempFile);
        }
    }
}
