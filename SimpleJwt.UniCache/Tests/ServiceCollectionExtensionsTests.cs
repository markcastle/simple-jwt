using System;
using Microsoft.Extensions.DependencyInjection;
using SimpleJwt.Abstractions.Caching;
using UniCache;
using Xunit;

namespace SimpleJwt.UniCache.Tests
{
    /// <summary>
    /// Unit tests for ServiceCollectionExtensions DI registration methods.
    /// </summary>
    public class ServiceCollectionExtensionsTests
    {
        /// <summary>
        /// Verifies that AddUniCacheTokenRepository registers UniCacheTokenRepository with a custom IUniCache and encryption settings.
        /// </summary>
        [Fact]
        public void ShouldRegisterWithCustomUniCacheAndEncryption()
        {
            var services = new ServiceCollection();
            var customCache = new MemoryUniCache();
            var encryption = new CacheEncryptionSettings(new byte[32], new byte[16], 100_000);
            services.AddUniCacheTokenRepository(customCache, encryption);
            var provider = services.BuildServiceProvider();
            var repo = provider.GetRequiredService<ITokenCacheStorage>() as UniCacheTokenRepository;
            Assert.NotNull(repo);
        }

        /// <summary>
        /// Verifies that AddInMemoryUniCacheTokenRepository registers UniCacheTokenRepository with an in-memory cache.
        /// </summary>
        [Fact]
        public void ShouldRegisterWithInMemoryUniCache()
        {
            var services = new ServiceCollection();
            services.AddInMemoryUniCacheTokenRepository();
            var provider = services.BuildServiceProvider();
            var repo = provider.GetRequiredService<ITokenCacheStorage>() as UniCacheTokenRepository;
            Assert.NotNull(repo);
        }

        /// <summary>
        /// Ensures AddUniCacheTokenRepository throws ArgumentNullException for null arguments.
        /// </summary>
        [Fact]
        public void ShouldThrowOnNullArguments()
        {
            var services = new ServiceCollection();
            Assert.Throws<ArgumentNullException>(() => ServiceCollectionExtensions.AddUniCacheTokenRepository(null, new MemoryUniCache()));
            Assert.Throws<ArgumentNullException>(() => ServiceCollectionExtensions.AddUniCacheTokenRepository(services, null));
        }

        /// <summary>
        /// Verifies UseUniCache registers a persistent UniCacheTokenRepository with encryption settings.
        /// </summary>
        [Fact]
        public void ShouldRegisterWithUseUniCache()
        {
            var services = new ServiceCollection();
            var cache = new MemoryUniCache();
            var encryption = new CacheEncryptionSettings(new byte[32], new byte[16], 100_000);
            services.UseUniCache(() => cache, () => encryption);
            var provider = services.BuildServiceProvider();
            var repo = provider.GetRequiredService<ITokenCacheStorage>() as UniCacheTokenRepository;
            Assert.NotNull(repo);
        }

        /// <summary>
        /// Verifies UseCustomCache registers a custom ITokenCacheStorage implementation.
        /// </summary>
        private class CustomCache : ITokenCacheStorage
        {
            public Task SetTokenAsync(string key, IJwtToken token, TimeSpan? expires = null, System.Threading.CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task<IJwtToken> GetTokenAsync(string key, System.Threading.CancellationToken cancellationToken = default) => Task.FromResult<IJwtToken>(null);
            public Task RemoveTokenAsync(string key, System.Threading.CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task ClearAsync(System.Threading.CancellationToken cancellationToken = default) => Task.CompletedTask;
        }

        [Fact]
        public void ShouldRegisterWithUseCustomCache()
        {
            var services = new ServiceCollection();
            services.UseCustomCache<CustomCache>();
            var provider = services.BuildServiceProvider();
            var repo = provider.GetRequiredService<ITokenCacheStorage>();
            Assert.IsType<CustomCache>(repo);
        }
    }
}
