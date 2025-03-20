using System;

namespace SimpleJwt.Abstractions.Validation
{
    /// <summary>
    /// Represents an error that occurred during JWT token validation.
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationError"/> class.
        /// </summary>
        /// <param name="code">The error code.</param>
        /// <param name="message">The error message.</param>
        public ValidationError(string code, string message)
        {
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentException("Error code cannot be null or empty.", nameof(code));
            }

            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentException("Error message cannot be null or empty.", nameof(message));
            }

            Code = code;
            Message = message;
        }

        /// <summary>
        /// Gets the error code.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets or sets additional details about the error.
        /// </summary>
        public object Details { get; set; }

        /// <summary>
        /// Returns a string representation of the validation error.
        /// </summary>
        /// <returns>A string representation of the validation error.</returns>
        public override string ToString()
        {
            return $"[{Code}] {Message}";
        }
    }
} 