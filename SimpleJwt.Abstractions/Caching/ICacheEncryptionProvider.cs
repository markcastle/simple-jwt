using System;

namespace SimpleJwt.Abstractions.Caching
{
    /// <summary>
    /// Abstraction for encrypting and decrypting cache values.
    /// </summary>
    public interface ICacheEncryptionProvider
    {
        /// <summary>
        /// Encrypts a byte array.
        /// </summary>
        /// <param name="data">The data to encrypt.</param>
        /// <returns>The encrypted data.</returns>
        byte[] Encrypt(byte[] data);

        /// <summary>
        /// Decrypts a byte array.
        /// </summary>
        /// <param name="encryptedData">The data to decrypt.</param>
        /// <returns>The decrypted data.</returns>
        byte[] Decrypt(byte[] encryptedData);
    }
}
