using System;
using SimpleJwt.Abstractions.Serialization;

namespace SimpleJwt.Core
{
    /// <summary>
    /// Provides configuration methods for the SimpleJwt library.
    /// </summary>
    public static class JwtConfiguration
    {
        private static bool _isInitialized;
        private static readonly object _lockObject = new object();
        
        /// <summary>
        /// Initializes the SimpleJwt library with default settings.
        /// Note: You must explicitly set a JSON provider before using SimpleJwt.
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized)
            {
                return;
            }
            
            lock (_lockObject)
            {
                if (_isInitialized)
                {
                    return;
                }
                
                CheckJsonProviderConfiguration();
                
                _isInitialized = true;
            }
        }
        
        /// <summary>
        /// Gets a value indicating whether the SimpleJwt library has been initialized.
        /// </summary>
        public static bool IsInitialized => _isInitialized;
        
        private static void CheckJsonProviderConfiguration()
        {
            if (!JsonProviderConfiguration.IsProviderConfigured)
            {
                throw new InvalidOperationException(
                    "No JSON provider has been configured. You must explicitly set a JSON provider before using SimpleJwt " +
                    "by calling JsonProviderConfiguration.SetProvider(). You can use either:\n" +
                    "1. new SimpleJwt.SystemTextJson.Serialization.SystemTextJsonProvider() (from SimpleJwt.SystemTextJson package)\n" +
                    "2. new SimpleJwt.Newtonsoft.Serialization.NewtonsoftJsonProvider() (from SimpleJwt.Newtonsoft package)\n" +
                    "3. Or your own custom IJsonProvider implementation."
                );
            }
        }
    }
} 