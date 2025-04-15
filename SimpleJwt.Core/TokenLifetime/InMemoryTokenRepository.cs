using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SimpleJwt.Abstractions.TokenRepository;

namespace SimpleJwt.Core.TokenLifetime
{
    /// <summary>
    /// Provides an in-memory implementation of <see cref="ITokenRepository"/>.
    /// </summary>
    public class InMemoryTokenRepository : ITokenRepository
    {
        private readonly ConcurrentDictionary<string, TokenInfo> _tokens = new ConcurrentDictionary<string, TokenInfo>();
        private readonly SemaphoreSlim _cleanupLock = new SemaphoreSlim(1, 1);
        private readonly TimeSpan _cleanupInterval;
        private DateTimeOffset _lastCleanup = DateTimeOffset.UtcNow;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryTokenRepository"/> class.
        /// </summary>
        /// <param name="cleanupInterval">The interval at which to automatically clean up expired tokens.</param>
        public InMemoryTokenRepository(TimeSpan? cleanupInterval = null)
        {
            _cleanupInterval = cleanupInterval ?? TimeSpan.FromMinutes(10);
        }

        /// <inheritdoc />
        public bool StoreToken(string token, string userId, DateTimeOffset expirationTime, string tokenType = "access", IDictionary<string, object> metadata = null)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var tokenInfo = new TokenInfo(token, userId, expirationTime, tokenType, metadata);
            bool result = _tokens.TryAdd(token, tokenInfo);
            
            RunCleanupIfNeeded();
            
            return result;
        }

        /// <inheritdoc />
        public Task<bool> StoreTokenAsync(string token, string userId, DateTimeOffset expirationTime, string tokenType = "access", IDictionary<string, object> metadata = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(StoreToken(token, userId, expirationTime, tokenType, metadata));
        }

        /// <inheritdoc />
        public TokenInfo GetToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            if (_tokens.TryGetValue(token, out var tokenInfo))
            {
                return tokenInfo;
            }

            return null;
        }

        /// <inheritdoc />
        public Task<TokenInfo> GetTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(GetToken(token));
        }

        /// <inheritdoc />
        public IEnumerable<TokenInfo> GetTokensForUser(string userId, string tokenType = null)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            return _tokens.Values
                .Where(t => t.UserId == userId && (tokenType == null || t.TokenType == tokenType))
                .ToList();
        }

        /// <inheritdoc />
        public Task<IEnumerable<TokenInfo>> GetTokensForUserAsync(string userId, string tokenType = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(GetTokensForUser(userId, tokenType));
        }

        /// <inheritdoc />
        public bool RemoveToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            return _tokens.TryRemove(token, out _);
        }

        /// <inheritdoc />
        public Task<bool> RemoveTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(RemoveToken(token));
        }

        /// <inheritdoc />
        public int RemoveTokensForUser(string userId, string tokenType = null)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var tokensToRemove = _tokens
                .Where(kvp => kvp.Value.UserId == userId && (tokenType == null || kvp.Value.TokenType == tokenType))
                .ToList();

            int count = 0;
            foreach (var kvp in tokensToRemove)
            {
                if (_tokens.TryRemove(kvp.Key, out _))
                {
                    count++;
                }
            }

            return count;
        }

        /// <inheritdoc />
        public Task<int> RemoveTokensForUserAsync(string userId, string tokenType = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(RemoveTokensForUser(userId, tokenType));
        }

        /// <inheritdoc />
        public int RemoveExpiredTokens(DateTimeOffset? beforeTime = null)
        {
            var now = beforeTime ?? DateTimeOffset.UtcNow;
            
            var expiredTokens = _tokens
                .Where(kvp => kvp.Value.ExpirationTime <= now)
                .ToList();

            int count = 0;
            foreach (var kvp in expiredTokens)
            {
                if (_tokens.TryRemove(kvp.Key, out _))
                {
                    count++;
                }
            }

            _lastCleanup = DateTimeOffset.UtcNow;
            return count;
        }

        /// <inheritdoc />
        public async Task<int> RemoveExpiredTokensAsync(DateTimeOffset? beforeTime = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            // Use a semaphore to prevent multiple cleanup operations from running concurrently
            await _cleanupLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            
            try
            {
                return RemoveExpiredTokens(beforeTime);
            }
            finally
            {
                _cleanupLock.Release();
            }
        }

        /// <inheritdoc />
        public bool TokenExists(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            return _tokens.ContainsKey(token);
        }

        /// <inheritdoc />
        public Task<bool> TokenExistsAsync(string token, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(TokenExists(token));
        }

        /// <inheritdoc />
        public int GetTokenCount(string userId = null, string tokenType = null)
        {
            if (userId == null && tokenType == null)
            {
                return _tokens.Count;
            }

            return _tokens.Values
                .Count(t => (userId == null || t.UserId == userId) && (tokenType == null || t.TokenType == tokenType));
        }

        /// <inheritdoc />
        public Task<int> GetTokenCountAsync(string userId = null, string tokenType = null, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(GetTokenCount(userId, tokenType));
        }

        /// <summary>
        /// Runs the cleanup operation if enough time has passed since the last cleanup.
        /// </summary>
        private void RunCleanupIfNeeded()
        {
            // Only clean up if enough time has passed since the last cleanup
            if ((DateTimeOffset.UtcNow - _lastCleanup) >= _cleanupInterval)
            {
                // Run cleanup on a background thread to avoid blocking the caller
                Task.Run(async () => await RemoveExpiredTokensAsync());
            }
        }
    }
} 