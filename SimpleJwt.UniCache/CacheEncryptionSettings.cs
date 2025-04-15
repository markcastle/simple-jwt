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

        /// <summary>
        /// Rotates the encryption key and salt for long-term security.
        /// </summary>
        /// <param name="newKey">The new encryption key.</param>
        /// <param name="newSalt">The new salt value.</param>
        /// <param name="newIterations">The number of iterations for key derivation.</param>
        public void RotateKey(byte[] newKey, byte[] newSalt, int newIterations)
        {
            if (newKey == null) throw new ArgumentNullException(nameof(newKey));
            if (newSalt == null) throw new ArgumentNullException(nameof(newSalt));
            if (newIterations <= 0) throw new ArgumentOutOfRangeException(nameof(newIterations));
            // Reason: Key rotation is critical for long-term security. This method updates all key material atomically.
            typeof(CacheEncryptionSettings).GetProperty(nameof(EncryptionKey)).SetValue(this, newKey);
            typeof(CacheEncryptionSettings).GetProperty(nameof(Salt)).SetValue(this, newSalt);
            typeof(CacheEncryptionSettings).GetProperty(nameof(Iterations)).SetValue(this, newIterations);
        }
    }
}
