using SimpleJwt.Abstractions.Validation;

namespace SimpleJwt.DependencyInjection
{
    /// <summary>
    /// Options for configuring SimpleJwt.
    /// </summary>
    public class SimpleJwtOptions
    {
        /// <summary>
        /// Gets or sets the parameters used to validate JWT tokens.
        /// </summary>
        public ValidationParameters ValidationParameters { get; set; } = new ValidationParameters();
    }
} 