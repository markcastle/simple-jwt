using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using SimpleJwt.Abstractions.Caching;
using Xunit;

namespace SimpleJwt.UniCache.Tests
{
    /// <summary>
    /// Unit tests for UniCacheTokenRepository.
    /// </summary>
    public class UniCacheTokenRepositoryTests
    {
        /// <summary>
        /// Verifies that SetTokenAsync and GetTokenAsync store and retrieve tokens as expected.
        /// </summary>
        [Fact]
        public async Task ShouldStoreAndRetrieveToken()
        {
            var repo = new UniCacheTokenRepository();
            var token = Mock.Of<IJwtToken>();
            await repo.SetTokenAsync("foo", token);
            var result = await repo.GetTokenAsync("foo");
            Assert.Equal(token, result);
        }

        /// <summary>
        /// Ensures that GetTokenAsync returns null for missing keys.
        /// </summary>
        [Fact]
        public async Task ShouldReturnNullForMissingKey()
        {
            var repo = new UniCacheTokenRepository();
            var result = await repo.GetTokenAsync("missing");
            Assert.Null(result);
        }

        /// <summary>
        /// Verifies that RemoveTokenAsync removes tokens from the repository.
        /// </summary>
        [Fact]
        public async Task ShouldRemoveToken()
        {
            var repo = new UniCacheTokenRepository();
            var token = Mock.Of<IJwtToken>();
            await repo.SetTokenAsync("foo", token);
            await repo.RemoveTokenAsync("foo");
            var result = await repo.GetTokenAsync("foo");
            Assert.Null(result);
        }

        /// <summary>
        /// Ensures that ClearAsync removes all tokens from the repository.
        /// </summary>
        [Fact]
        public async Task ShouldClearAllTokens()
        {
            var repo = new UniCacheTokenRepository();
            var token = Mock.Of<IJwtToken>();
            await repo.SetTokenAsync("foo", token);
            await repo.SetTokenAsync("bar", token);
            await repo.ClearAsync();
            Assert.Null(await repo.GetTokenAsync("foo"));
            Assert.Null(await repo.GetTokenAsync("bar"));
        }

        /// <summary>
        /// Ensures that token expiration is respected.
        /// </summary>
        [Fact]
        public async Task ShouldRespectExpiration()
        {
            var repo = new UniCacheTokenRepository();
            var token = Mock.Of<IJwtToken>();
            await repo.SetTokenAsync("foo", token, TimeSpan.FromMilliseconds(50));
            await Task.Delay(100);
            var result = await repo.GetTokenAsync("foo");
            Assert.Null(result);
        }

        /// <summary>
        /// Ensures that methods throw ArgumentNullException when passed null keys or tokens.
        /// </summary>
        [Fact]
        public async Task ShouldThrowOnNullKeyOrToken()
        {
            var repo = new UniCacheTokenRepository();
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.SetTokenAsync(null, Mock.Of<IJwtToken>()));
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.SetTokenAsync("foo", null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.GetTokenAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.RemoveTokenAsync(null));
        }

        /// <summary>
        /// Ensures that cancellation tokens are respected by all async methods.
        /// </summary>
        [Fact]
        public async Task ShouldSupportCancellation()
        {
            var repo = new UniCacheTokenRepository();
            var cts = new CancellationTokenSource();
            cts.Cancel();
            await Assert.ThrowsAsync<OperationCanceledException>(() => repo.GetTokenAsync("foo", cts.Token));
            await Assert.ThrowsAsync<OperationCanceledException>(() => repo.SetTokenAsync("foo", Mock.Of<IJwtToken>(), null, cts.Token));
            await Assert.ThrowsAsync<OperationCanceledException>(() => repo.RemoveTokenAsync("foo", cts.Token));
            await Assert.ThrowsAsync<OperationCanceledException>(() => repo.ClearAsync(cts.Token));
        }
    }
}
