using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using SimpleJwt.Abstractions.Serialization;
using SimpleJwt.SystemTextJson.Serialization;

namespace SimpleJwt.SystemTextJson.DependencyInjection
{
    /// <summary>
    /// Extension methods for registering SimpleJwt.SystemTextJson with Microsoft DI.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configures SimpleJwt to use System.Text.Json for serialization.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddSimpleJwtSystemTextJson(this IServiceCollection services)
        {
            return AddSimpleJwtSystemTextJson(services, options => { });
        }

        /// <summary>
        /// Configures SimpleJwt to use System.Text.Json for serialization with custom options.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureOptions">A delegate to configure the System.Text.Json options.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddSimpleJwtSystemTextJson(
            this IServiceCollection services, 
            Action<JsonSerializerOptions> configureOptions)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            configureOptions(options);
            
            var provider = new SystemTextJsonProvider(options);
            services.AddSingleton<IJsonProvider>(provider);
            
            // Configure JsonProviderConfiguration for non-DI scenarios
            JsonProviderConfiguration.SetProvider(provider);
            
            return services;
        }

        /// <summary>
        /// Configures SimpleJwt to use a custom System.Text.Json provider.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="providerFactory">A factory function to create the provider.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddSimpleJwtSystemTextJson(
            this IServiceCollection services,
            Func<IServiceProvider, SystemTextJsonProvider> providerFactory)
        {
            services.AddSingleton<IJsonProvider>(sp =>
            {
                var provider = providerFactory(sp);
                
                // Configure JsonProviderConfiguration for non-DI scenarios
                JsonProviderConfiguration.SetProvider(provider);
                
                return provider;
            });
            
            return services;
        }
    }
} 