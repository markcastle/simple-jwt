using System;
using System.Threading;
using System.Threading.Tasks;
using SimpleJwt.Abstractions.Caching;
using UniCache;
using UniCache.Encryption;
using Newtonsoft.Json;

namespace SimpleJwt.UniCache
{
    /// <summary>
    /// A UniCache-based implementation of ITokenCacheStorage for persistent and encrypted JWT token storage.
    /// </summary>
    public class UniCacheTokenRepository : ITokenCacheStorage
    {
        private readonly IUniCache _cache;
        private readonly ICacheEncryptionSettings _encryptionSettings;

        /// <summary>
        /// Initializes a new instance of the UniCacheTokenRepository class.
        /// </summary>
        /// <param name="cache">The UniCache instance to use for persistence.</param>
        /// <param name="encryptionSettings">Encryption settings for secure token storage.</param>
        public UniCacheTokenRepository(IUniCache cache = null, ICacheEncryptionSettings encryptionSettings = null)
        {
            _cache = cache ?? new MemoryUniCache();
            _encryptionSettings = encryptionSettings;
        }

        public async Task SetTokenAsync(string key, IJwtToken token, TimeSpan? expires = null, CancellationToken cancellationToken = default)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (token == null) throw new ArgumentNullException(nameof(token));
            cancellationToken.ThrowIfCancellationRequested();

            // Serialize and encrypt token if encryption is enabled
            var serialized = JsonConvert.SerializeObject(token);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(serialized);
            if (_encryptionSettings != null)
            {
                data = UniCache.Encryption.AesEncryption.Encrypt(
                    data,
                    _encryptionSettings.EncryptionKey,
                    _encryptionSettings.Salt,
                    _encryptionSettings.Iterations);
            }
            await _cache.SetAsync(key, data, expires, cancellationToken);
        }

        public async Task<IJwtToken> GetTokenAsync(string key, CancellationToken cancellationToken = default)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            cancellationToken.ThrowIfCancellationRequested();

            var data = await _cache.GetAsync<byte[]>(key, cancellationToken);
            if (data == null) return null;
            if (_encryptionSettings != null)
            {
                data = UniCache.Encryption.AesEncryption.Decrypt(
                    data,
                    _encryptionSettings.EncryptionKey,
                    _encryptionSettings.Salt,
                    _encryptionSettings.Iterations);
            }
            var json = System.Text.Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<IJwtToken>(json);
        }

        public async Task RemoveTokenAsync(string key, CancellationToken cancellationToken = default)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            cancellationToken.ThrowIfCancellationRequested();
            await _cache.RemoveAsync(key, cancellationToken);
        }

        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _cache.ClearAsync(cancellationToken);
        }
    }
}
