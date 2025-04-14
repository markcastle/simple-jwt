using System;
using SimpleJwt.Abstractions.Serialization;
using SimpleJwt.Core;
using SimpleJwt.SystemTextJson.Serialization;
using SimpleJwt.Newtonsoft.Serialization;
using Xunit;

namespace SimpleJwt.Tests
{
    /// <summary>
    /// Base class for all SimpleJwt tests that provides common setup and teardown functionality.
    /// </summary>
    public abstract class TestBase : IDisposable
    {
        private readonly IJsonProvider? _originalProvider;
        private readonly bool _useSystemTextJson;

        protected TestBase(bool useSystemTextJson = true)
        {
            _useSystemTextJson = useSystemTextJson;
            _originalProvider = JsonProviderConfiguration.IsProviderConfigured 
                ? JsonProviderConfiguration.GetProvider() 
                : null;

            // Set up the JSON provider
            if (useSystemTextJson)
            {
                JsonProviderConfiguration.SetProvider(new SystemTextJsonProvider());
            }
            else
            {
                JsonProviderConfiguration.SetProvider(new NewtonsoftJsonProvider());
            }

            // Initialize the factories with default implementations
            SimpleJwtDefaults.Initialize();
        }

        public void Dispose()
        {
            // Restore the original provider
            if (_originalProvider != null)
            {
                JsonProviderConfiguration.SetProvider(_originalProvider);
            }
            else
            {
                // If there was no original provider, we need to clear the current one
                // by creating a new instance of SystemTextJsonProvider
                JsonProviderConfiguration.SetProvider(new SystemTextJsonProvider());
            }
        }
    }
} 