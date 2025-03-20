using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleJwt.Abstractions.Validation
{
    /// <summary>
    /// Provides functionality to transform JWT claims during token validation.
    /// </summary>
    public interface IClaimsTransformer
    {
        /// <summary>
        /// Transforms JWT claims during token validation.
        /// </summary>
        /// <param name="claims">The original claims from the token.</param>
        /// <param name="context">The validation context.</param>
        /// <returns>The transformed claims.</returns>
        IDictionary<string, object> TransformClaims(IDictionary<string, object> claims, ValidationContext context);

        /// <summary>
        /// Asynchronously transforms JWT claims during token validation.
        /// </summary>
        /// <param name="claims">The original claims from the token.</param>
        /// <param name="context">The validation context.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous transformation operation. The task result contains the transformed claims.</returns>
        Task<IDictionary<string, object>> TransformClaimsAsync(
            IDictionary<string, object> claims, 
            ValidationContext context, 
            CancellationToken cancellationToken = default);
    }
} 