using System;
using Microsoft.Extensions.DependencyInjection;
using SimpleJwt.Abstractions.Caching;

namespace SimpleJwt.UniCache
{
    /// <summary>
    /// Extension methods for registering UniCache-based token storage with DI.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers UniCacheTokenRepository as the ITokenCacheStorage implementation.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddUniCacheTokenRepository(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            services.AddSingleton<ITokenCacheStorage, UniCacheTokenRepository>();
            return services;
        }
    }
}
