using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleJwt.Abstractions;
using SimpleJwt.Abstractions.Serialization;
using SimpleJwt.Abstractions.Validation;
using SimpleJwt.Core;
using SimpleJwt.Core.TokenLifetime;
using SimpleJwt.Core.Validation;

#if SYSTEMTEXTJSON_AVAILABLE
using SystemTextJsonProvider = SimpleJwt.SystemTextJson.Serialization.SystemTextJsonProvider;
#endif

namespace SimpleJwt.DependencyInjection
{
    /// <summary>
    /// Extension methods for registering SimpleJwt services with Microsoft DI.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds SimpleJwt core services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureOptions">Optional action to configure SimpleJwt options.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddSimpleJwt(
            this IServiceCollection services,
            Action<SimpleJwtOptions> configureOptions = null)
        {
            var options = new SimpleJwtOptions();
            configureOptions?.Invoke(options);

            // Register core services
            services.TryAddSingleton<IJwtParser, JwtParser>();
            services.TryAddSingleton<IJwtBuilder, JwtBuilder>();
            
            // Register validator with configuration
            services.TryAddSingleton<IJwtValidator>(sp => 
            {
                var parser = sp.GetRequiredService<IJwtParser>();
                var validator = new JwtValidator(parser);
                
                if (options.ValidationParameters != null)
                {
                    if (options.ValidationParameters.SymmetricSecurityKey != null)
                    {
                        validator.SetHmacKey(options.ValidationParameters.SymmetricSecurityKey);
                    }
                    
                    if (!string.IsNullOrEmpty(options.ValidationParameters.ValidIssuer))
                    {
                        validator.SetIssuer(options.ValidationParameters.ValidIssuer);
                    }
                    
                    if (!string.IsNullOrEmpty(options.ValidationParameters.ValidAudience))
                    {
                        validator.SetAudience(options.ValidationParameters.ValidAudience);
                    }
                    
                    if (options.ValidationParameters.ClockSkew != default)
                    {
                        validator.SetClockSkew(options.ValidationParameters.ClockSkew);
                    }
                    
                    if (options.ValidationParameters.ValidateLifetime)
                    {
                        validator.ValidateExpiration();
                        validator.ValidateNotBefore();
                    }
                }
                
                return validator;
            });
            
            return services;
        }
        
        /// <summary>
        /// Adds token refresher services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddTokenRefresher(this IServiceCollection services)
        {
            services.TryAddSingleton<ITokenRefresher>(sp => 
            {
                var validator = sp.GetRequiredService<IJwtValidator>();
                var parser = sp.GetRequiredService<IJwtParser>();
                return new JwtRefresher(validator, parser);
            });
            return services;
        }
        
        /// <summary>
        /// Adds token revoker services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddTokenRevoker(this IServiceCollection services)
        {
            services.TryAddSingleton<ITokenRevoker>(sp =>
            {
                var parser = sp.GetRequiredService<IJwtParser>();
                return new JwtRevoker(parser);
            });
            return services;
        }

#if SYSTEMTEXTJSON_AVAILABLE
        /// <summary>
        /// Configures SimpleJwt to use System.Text.Json for serialization.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureProvider">Optional action to configure the System.Text.Json provider.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection UseSystemTextJson(
            this IServiceCollection services,
            Action<SystemTextJsonProvider> configureProvider = null)
        {
            var provider = new SystemTextJsonProvider();
            configureProvider?.Invoke(provider);
            JsonProviderConfiguration.SetProvider(provider);
            return services;
        }
#else
        /// <summary>
        /// Configures SimpleJwt to use System.Text.Json for serialization.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureProvider">Optional action to configure the System.Text.Json provider.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection UseSystemTextJson(
            this IServiceCollection services,
            Action<IJsonProvider> configureProvider = null)
        {
            throw new InvalidOperationException(
                "The SystemTextJson provider is not available. To use System.Text.Json, make sure you have added a " +
                "reference to the SimpleJwt.SystemTextJson package and that the DependencyInjection project has a " +
                "project reference to it.");
        }
#endif

        /// <summary>
        /// Configures SimpleJwt to use Newtonsoft.Json for serialization.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureProvider">Optional action to configure the Newtonsoft.Json provider.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection UseNewtonsoftJson(
            this IServiceCollection services,
            Action<IJsonProvider> configureProvider = null)
        {
            // Since we can't reference SimpleJwt.Newtonsoft directly (to avoid forcing a dependency),
            // we'll use reflection to create the provider. If the project has a reference to SimpleJwt.Newtonsoft,
            // this will work; otherwise, it will throw an exception with a helpful message.
            try
            {
                var newtonsoftProviderType = Type.GetType("SimpleJwt.Newtonsoft.Serialization.NewtonsoftJsonProvider, SimpleJwt.Newtonsoft");
                if (newtonsoftProviderType == null)
                {
                    throw new InvalidOperationException(
                        "The NewtonsoftJsonProvider type was not found. Make sure you have added a reference to the SimpleJwt.Newtonsoft package.");
                }
                
                var provider = Activator.CreateInstance(newtonsoftProviderType) as IJsonProvider;
                if (provider == null)
                {
                    throw new InvalidOperationException(
                        "Failed to create an instance of NewtonsoftJsonProvider. Make sure you have added a reference to the SimpleJwt.Newtonsoft package.");
                }
                
                configureProvider?.Invoke(provider);
                JsonProviderConfiguration.SetProvider(provider);
                
                return services;
            }
            catch (Exception ex) when (!(ex is InvalidOperationException))
            {
                throw new InvalidOperationException(
                    "An error occurred while configuring Newtonsoft.Json. Make sure you have added a reference to the SimpleJwt.Newtonsoft package.",
                    ex);
            }
        }
    }
} 