using SimpleJwt.Abstractions;
using SimpleJwt.Abstractions.Validation;
using SimpleJwt.Core.Validation;

namespace SimpleJwt.Core
{
    /// <summary>
    /// Provides methods to initialize the factories with default implementations.
    /// </summary>
    public static class SimpleJwtDefaults
    {
        /// <summary>
        /// Initializes all factories with their default implementations.
        /// </summary>
        public static void Initialize()
        {
            InitializeJwtBuilder();
            InitializeJwtParser();
            InitializeJwtValidator();
        }

        /// <summary>
        /// Initializes the JWT builder factory with the default implementation.
        /// </summary>
        public static void InitializeJwtBuilder()
        {
            JwtBuilderFactory.SetFactory(() => new JwtBuilder());
        }

        /// <summary>
        /// Initializes the JWT parser factory with the default implementation.
        /// </summary>
        public static void InitializeJwtParser()
        {
            JwtParserFactory.SetFactory(() => new JwtParser());
        }

        /// <summary>
        /// Initializes the JWT validator factory with the default implementation.
        /// </summary>
        public static void InitializeJwtValidator()
        {
            JwtValidatorFactory.SetFactory(() => new JwtValidator(JwtParserFactory.Create()));
        }
    }
} 