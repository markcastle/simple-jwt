using System;

namespace SimpleJwt.Abstractions
{
    /// <summary>
    /// Provides factory methods for creating instances of JWT parsers.
    /// </summary>
    public static class JwtParserFactory
    {
        private static Func<IJwtParser> _factory;

        /// <summary>
        /// Sets the factory method for creating JWT parsers.
        /// </summary>
        /// <param name="factory">The factory method that creates JWT parsers.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory"/> is null.</exception>
        public static void SetFactory(Func<IJwtParser> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>
        /// Creates a new <see cref="IJwtParser"/> instance.
        /// </summary>
        /// <returns>A new <see cref="IJwtParser"/> instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the factory method has not been set.</exception>
        public static IJwtParser Create()
        {
            if (_factory == null)
            {
                throw new InvalidOperationException("JWT parser factory has not been set. Call SetFactory first.");
            }

            return _factory();
        }
    }
} 