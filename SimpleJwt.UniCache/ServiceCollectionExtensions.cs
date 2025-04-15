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

        /// <summary>
        /// Registers UniCacheTokenRepository with a custom IUniCache instance (persistent or in-memory) and optional encryption settings.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="uniCache">The IUniCache instance to use (e.g., persistent or in-memory).</param>
        /// <param name="encryptionSettings">Optional encryption settings for secure token storage.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddUniCacheTokenRepository(this IServiceCollection services, IUniCache uniCache, ICacheEncryptionSettings encryptionSettings = null)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (uniCache == null) throw new ArgumentNullException(nameof(uniCache));
            services.AddSingleton<ITokenCacheStorage>(sp => new UniCacheTokenRepository(uniCache, encryptionSettings));
            return services;
        }

        /// <summary>
        /// Registers UniCacheTokenRepository with an in-memory UniCache instance (no persistence, for testing or ephemeral use).
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection for chaining.</returns>
        public static IServiceCollection AddInMemoryUniCacheTokenRepository(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            services.AddSingleton<ITokenCacheStorage>(sp => new UniCacheTokenRepository(new MemoryUniCache()));
            return services;
        }
    }
}
