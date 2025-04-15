using System;
using System.Text;
using System.Threading.Tasks;
using SimpleJwt.Abstractions.Caching;
using Xunit;

namespace SimpleJwt.UniCache.Tests
{
    /// <summary>
    /// Tests for AES encryption, decryption, and key rotation in UniCache integration.
    /// </summary>
    public class EncryptionTests
    {
        /// <summary>
        /// Verifies that encrypted data can be decrypted with the correct key and salt.
        /// </summary>
        [Fact]
        public void ShouldEncryptAndDecryptWithCorrectKey()
        {
            var key = new byte[32];
            var salt = new byte[16];
            new Random().NextBytes(key);
            new Random().NextBytes(salt);
            var settings = new CacheEncryptionSettings(key, salt);
            var data = Encoding.UTF8.GetBytes("secret-token");
            var encrypted = UniCache.Encryption.AesEncryption.Encrypt(data, key, salt, settings.Iterations);
            var decrypted = UniCache.Encryption.AesEncryption.Decrypt(encrypted, key, salt, settings.Iterations);
            Assert.Equal(data, decrypted);
        }

        /// <summary>
        /// Ensures that decryption fails with a wrong key or salt.
        /// </summary>
        [Fact]
        public void ShouldFailDecryptionWithWrongKeyOrSalt()
        {
            var key = new byte[32];
            var salt = new byte[16];
            new Random().NextBytes(key);
            new Random().NextBytes(salt);
            var wrongKey = new byte[32];
            var wrongSalt = new byte[16];
            new Random().NextBytes(wrongKey);
            new Random().NextBytes(wrongSalt);
            var settings = new CacheEncryptionSettings(key, salt);
            var data = Encoding.UTF8.GetBytes("secret-token");
            var encrypted = UniCache.Encryption.AesEncryption.Encrypt(data, key, salt, settings.Iterations);
            Assert.ThrowsAny<Exception>(() => UniCache.Encryption.AesEncryption.Decrypt(encrypted, wrongKey, salt, settings.Iterations));
            Assert.ThrowsAny<Exception>(() => UniCache.Encryption.AesEncryption.Decrypt(encrypted, key, wrongSalt, settings.Iterations));
        }

        /// <summary>
        /// Verifies key rotation updates the key and salt, and that old data cannot be decrypted with new keys.
        /// </summary>
        [Fact]
        public void ShouldRotateKeyAndInvalidateOldData()
        {
            var key = new byte[32];
            var salt = new byte[16];
            new Random().NextBytes(key);
            new Random().NextBytes(salt);
            var settings = new CacheEncryptionSettings(key, salt);
            var data = Encoding.UTF8.GetBytes("secret-token");
            var encrypted = UniCache.Encryption.AesEncryption.Encrypt(data, key, salt, settings.Iterations);
            // Rotate key
            var newKey = new byte[32];
            var newSalt = new byte[16];
            new Random().NextBytes(newKey);
            new Random().NextBytes(newSalt);
            settings.RotateKey(newKey, newSalt, 200_000);
            Assert.ThrowsAny<Exception>(() => UniCache.Encryption.AesEncryption.Decrypt(encrypted, settings.EncryptionKey, settings.Salt, settings.Iterations));
        }
    }
}
