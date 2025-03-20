using System;

namespace SimpleJwt.Abstractions.Validation
{
    /// <summary>
    /// Provides factory methods for creating instances of JWT validators.
    /// </summary>
    public static class JwtValidatorFactory
    {
        private static Func<IJwtValidator> _factory;

        /// <summary>
        /// Sets the factory method for creating JWT validators.
        /// </summary>
        /// <param name="factory">The factory method that creates JWT validators.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory"/> is null.</exception>
        public static void SetFactory(Func<IJwtValidator> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>
        /// Creates a new <see cref="IJwtValidator"/> instance.
        /// </summary>
        /// <returns>A new <see cref="IJwtValidator"/> instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the factory method has not been set.</exception>
        public static IJwtValidator Create()
        {
            if (_factory == null)
            {
                throw new InvalidOperationException("JWT validator factory has not been set. Call SetFactory first.");
            }

            return _factory();
        }
    }
} 