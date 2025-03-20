using System;
using System.Collections.Generic;

namespace SimpleJwt.Abstractions.Validation
{
    /// <summary>
    /// Represents the result of a JWT token validation operation.
    /// </summary>
    public class ValidationResult
    {
        private readonly List<ValidationError> _errors;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResult"/> class.
        /// </summary>
        /// <param name="isValid">A value indicating whether the token is valid.</param>
        public ValidationResult(bool isValid)
        {
            IsValid = isValid;
            _errors = new List<ValidationError>();
        }

        /// <summary>
        /// Gets a value indicating whether the token is valid.
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        /// Gets the validation errors that occurred during validation.
        /// </summary>
        public IReadOnlyList<ValidationError> Errors => _errors;

        /// <summary>
        /// Gets a value indicating whether the token has any validation errors.
        /// </summary>
        public bool HasErrors => _errors.Count > 0;

        /// <summary>
        /// Adds a validation error to the result.
        /// </summary>
        /// <param name="error">The validation error to add.</param>
        /// <returns>The current <see cref="ValidationResult"/> instance.</returns>
        public ValidationResult AddError(ValidationError error)
        {
            if (error == null)
            {
                throw new ArgumentNullException(nameof(error));
            }

            _errors.Add(error);
            return this;
        }

        /// <summary>
        /// Creates a successful validation result.
        /// </summary>
        /// <returns>A new <see cref="ValidationResult"/> instance indicating success.</returns>
        public static ValidationResult Success()
        {
            return new ValidationResult(true);
        }

        /// <summary>
        /// Creates a failed validation result with the specified error.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>A new <see cref="ValidationResult"/> instance indicating failure.</returns>
        public static ValidationResult Failure(string errorCode, string errorMessage)
        {
            var result = new ValidationResult(false);
            result.AddError(new ValidationError(errorCode, errorMessage));
            return result;
        }
    }
} 