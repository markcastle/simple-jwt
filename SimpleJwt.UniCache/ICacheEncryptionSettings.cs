namespace SimpleJwt.UniCache
{
    /// <summary>
    /// Represents encryption settings for cache persistence.
    /// </summary>
    public interface ICacheEncryptionSettings
    {
        /// <summary>
        /// Gets the encryption key (should be securely stored and rotated).
        /// </summary>
        byte[] EncryptionKey { get; }

        /// <summary>
        /// Gets the salt for key derivation.
        /// </summary>
        byte[] Salt { get; }

        /// <summary>
        /// Gets the number of iterations for key derivation.
        /// </summary>
        int Iterations { get; }
    }
}
