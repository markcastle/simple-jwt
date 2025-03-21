using System;

namespace SimpleJwt.Abstractions.Serialization
{
    /// <summary>
    /// Provides configuration options for JSON serialization in SimpleJwt.
    /// </summary>
    public static class JsonProviderConfiguration
    {
        private static IJsonProvider _defaultProvider;
        private static readonly object _lockObject = new object();
        
        /// <summary>
        /// Gets the current JSON provider.
        /// </summary>
        /// <returns>The configured JSON provider, or null if no provider has been set.</returns>
        public static IJsonProvider GetProvider()
        {
            if (_defaultProvider == null)
            {
                lock (_lockObject)
                {
                    if (_defaultProvider == null)
                    {
                        throw new InvalidOperationException(
                            "No JSON provider has been configured. Please call SetProvider before using any JWT functionality " +
                            "that requires JSON serialization or deserialization.");
                    }
                }
            }
            
            return _defaultProvider;
        }
        
        /// <summary>
        /// Sets the JSON provider to use for serialization and deserialization.
        /// </summary>
        /// <param name="provider">The JSON provider to use.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="provider"/> is null.</exception>
        public static void SetProvider(IJsonProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }
            
            lock (_lockObject)
            {
                _defaultProvider = provider;
            }
        }
        
        /// <summary>
        /// Returns true if a JSON provider has been configured.
        /// </summary>
        public static bool IsProviderConfigured => _defaultProvider != null;
    }
} 