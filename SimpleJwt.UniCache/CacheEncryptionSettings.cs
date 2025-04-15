using System;

namespace SimpleJwt.UniCache
{
    /// <summary>
    /// Default implementation of ICacheEncryptionSettings for AES encryption.
    /// </summary>
    public class CacheEncryptionSettings : ICacheEncryptionSettings
    {
        /// <summary>
        /// Gets the encryption key.
        /// </summary>
        public byte[] EncryptionKey { get; }

        /// <summary>
        /// Gets the salt for key derivation.
        /// </summary>
        public byte[] Salt { get; }

        /// <summary>
        /// Gets the number of iterations for key derivation.
        /// </summary>
        public int Iterations { get; }

        /// <summary>
        /// Initializes a new instance of the CacheEncryptionSettings class.
        /// </summary>
        /// <param name="encryptionKey">The encryption key (must be securely generated and stored).</param>
        /// <param name="salt">The salt for key derivation.</param>
        /// <param name="iterations">The number of iterations for key derivation (default: 100_000).</param>
        public CacheEncryptionSettings(byte[] encryptionKey, byte[] salt, int iterations = 100_000)
        {
            EncryptionKey = encryptionKey ?? throw new ArgumentNullException(nameof(encryptionKey));
            Salt = salt ?? throw new ArgumentNullException(nameof(salt));
            Iterations = iterations > 0 ? iterations : throw new ArgumentOutOfRangeException(nameof(iterations));
        }
    }
}
