using System;
using System.Threading;
using System.Threading.Tasks;
using SimpleJwt.Abstractions.Caching;
using SimpleJwt.Core.Caching;
using Xunit;

namespace SimpleJwt.Tests.Caching
{
    /// <summary>
    /// Unit tests for SimpleMemoryCacheProvider.
    /// </summary>
    public class SimpleMemoryCacheProviderTests
    {
        [Fact]
        public async Task ShouldStoreAndRetrieveValue()
        {
            var cache = new SimpleMemoryCacheProvider<string, string>();
            await cache.SetAsync("foo", "bar");
            var result = await cache.GetAsync("foo");
            Assert.Equal("bar", result);
        }

        [Fact]
        public async Task ShouldReturnDefaultForMissingKey()
        {
            var cache = new SimpleMemoryCacheProvider<string, string>();
            var result = await cache.GetAsync("missing");
            Assert.Null(result);
        }

        [Fact]
        public async Task ShouldRemoveValue()
        {
            var cache = new SimpleMemoryCacheProvider<string, string>();
            await cache.SetAsync("foo", "bar");
            await cache.RemoveAsync("foo");
            var result = await cache.GetAsync("foo");
            Assert.Null(result);
        }

        [Fact]
        public async Task ShouldClearAllValues()
        {
            var cache = new SimpleMemoryCacheProvider<string, string>();
            await cache.SetAsync("foo", "bar");
            await cache.SetAsync("baz", "qux");
            await cache.ClearAsync();
            Assert.Null(await cache.GetAsync("foo"));
            Assert.Null(await cache.GetAsync("baz"));
        }

        [Fact]
        public async Task ShouldRespectExpiration()
        {
            var cache = new SimpleMemoryCacheProvider<string, string>();
            await cache.SetAsync("foo", "bar", TimeSpan.FromMilliseconds(50));
            await Task.Delay(100);
            var result = await cache.GetAsync("foo");
            Assert.Null(result);
        }

        [Fact]
        public async Task ShouldEvictOldestWhenOverLimit()
        {
            var cache = new SimpleMemoryCacheProvider<string, string>(maxSize: 3);
            await cache.SetAsync("a", "1");
            await cache.SetAsync("b", "2");
            await cache.SetAsync("c", "3");
            await cache.SetAsync("d", "4"); // Should trigger eviction
            // 'a' should be evicted (FIFO)
            Assert.Null(await cache.GetAsync("a"));
            Assert.Equal("2", await cache.GetAsync("b"));
            Assert.Equal("3", await cache.GetAsync("c"));
            Assert.Equal("4", await cache.GetAsync("d"));
        }

        [Fact]
        public async Task ShouldThrowOnNullKeyOrValue()
        {
            var cache = new SimpleMemoryCacheProvider<string, string>();
            await Assert.ThrowsAsync<ArgumentNullException>(() => cache.SetAsync(null, "bar"));
            await Assert.ThrowsAsync<ArgumentNullException>(() => cache.SetAsync("foo", null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => cache.GetAsync(null));
            await Assert.ThrowsAsync<ArgumentNullException>(() => cache.RemoveAsync(null));
        }

        [Fact]
        public async Task ShouldSupportCancellation()
        {
            var cache = new SimpleMemoryCacheProvider<string, string>();
            var cts = new CancellationTokenSource();
            cts.Cancel();
            await Assert.ThrowsAsync<OperationCanceledException>(() => cache.GetAsync("foo", cts.Token));
            await Assert.ThrowsAsync<OperationCanceledException>(() => cache.SetAsync("foo", "bar", null, cts.Token));
            await Assert.ThrowsAsync<OperationCanceledException>(() => cache.RemoveAsync("foo", cts.Token));
            await Assert.ThrowsAsync<OperationCanceledException>(() => cache.ClearAsync(cts.Token));
        }
    }
}
