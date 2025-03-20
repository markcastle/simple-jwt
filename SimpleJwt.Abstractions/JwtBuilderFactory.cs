using System;

namespace SimpleJwt.Abstractions
{
    /// <summary>
    /// Provides factory methods for creating JWT builder instances.
    /// </summary>
    public static class JwtBuilderFactory
    {
        private static Func<IJwtBuilder> _factory;

        /// <summary>
        /// Sets the factory method used to create JWT builder instances.
        /// </summary>
        /// <param name="factory">The factory method.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory"/> is null.</exception>
        public static void SetFactory(Func<IJwtBuilder> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>
        /// Creates a new JWT builder instance.
        /// </summary>
        /// <returns>A new <see cref="IJwtBuilder"/> instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the factory method has not been set.</exception>
        public static IJwtBuilder Create()
        {
            if (_factory == null)
            {
                throw new InvalidOperationException(
                    "The JWT builder factory has not been set. Call JwtBuilderFactory.SetFactory first.");
            }

            return _factory();
        }
    }
} 