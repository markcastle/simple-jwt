using System;
using System.Collections.Generic;

namespace SimpleJwt.Abstractions
{
    /// <summary>
    /// Represents an immutable JWT token with its components.
    /// </summary>
    public interface IJwtToken
    {
        /// <summary>
        /// Gets the JWT header claims.
        /// </summary>
        IReadOnlyDictionary<string, object> Header { get; }

        /// <summary>
        /// Gets the JWT payload claims.
        /// </summary>
        IReadOnlyDictionary<string, object> Payload { get; }

        /// <summary>
        /// Gets the JWT signature.
        /// </summary>
        string Signature { get; }

        /// <summary>
        /// Gets the raw JWT token string.
        /// </summary>
        string RawToken { get; }

        /// <summary>
        /// Creates a new JWT token with the specified claim added or updated.
        /// </summary>
        /// <param name="name">The name of the claim.</param>
        /// <param name="value">The value of the claim.</param>
        /// <returns>A new JWT token with the updated claim.</returns>
        IJwtToken WithClaim(string name, object value);

        /// <summary>
        /// Creates a new JWT token with the specified claim removed.
        /// </summary>
        /// <param name="name">The name of the claim to remove.</param>
        /// <returns>A new JWT token without the specified claim.</returns>
        IJwtToken WithoutClaim(string name);

        /// <summary>
        /// Creates a new JWT token with the specified header claim added or updated.
        /// </summary>
        /// <param name="name">The name of the header claim.</param>
        /// <param name="value">The value of the header claim.</param>
        /// <returns>A new JWT token with the updated header claim.</returns>
        IJwtToken WithHeaderClaim(string name, object value);

        /// <summary>
        /// Creates a new JWT token with the specified header claim removed.
        /// </summary>
        /// <param name="name">The name of the header claim to remove.</param>
        /// <returns>A new JWT token without the specified header claim.</returns>
        IJwtToken WithoutHeaderClaim(string name);
        
        /// <summary>
        /// Gets a claim from the payload by name.
        /// </summary>
        /// <typeparam name="T">The expected type of the claim value.</typeparam>
        /// <param name="claimName">The name of the claim to retrieve.</param>
        /// <returns>The value of the claim if found and convertible to the specified type; otherwise, the default value for the type.</returns>
        T GetClaim<T>(string claimName);
        
        /// <summary>
        /// Attempts to get a claim from the payload by name.
        /// </summary>
        /// <typeparam name="T">The expected type of the claim value.</typeparam>
        /// <param name="claimName">The name of the claim to retrieve.</param>
        /// <param name="value">When this method returns, contains the value of the claim, if found; otherwise, the default value for the type of the value parameter.</param>
        /// <returns>true if the claim is found; otherwise, false.</returns>
        bool TryGetClaim<T>(string claimName, out T value);
        
        /// <summary>
        /// Attempts to get a header claim by name.
        /// </summary>
        /// <typeparam name="T">The expected type of the header claim value.</typeparam>
        /// <param name="claimName">The name of the header claim to retrieve.</param>
        /// <param name="value">When this method returns, contains the value of the header claim, if found; otherwise, the default value for the type of the value parameter.</param>
        /// <returns>true if the header claim is found; otherwise, false.</returns>
        bool TryGetHeaderClaim<T>(string claimName, out T value);
    }
} 