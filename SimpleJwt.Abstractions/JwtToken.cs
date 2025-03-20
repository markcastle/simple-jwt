using System;
using System.Collections.Generic;

namespace SimpleJwt.Abstractions
{
    /// <summary>
    /// Represents a JSON Web Token (JWT).
    /// </summary>
    public class JwtToken : IJwtToken
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JwtToken"/> class.
        /// </summary>
        /// <param name="header">The JWT header claims.</param>
        /// <param name="payload">The JWT payload claims.</param>
        /// <param name="rawToken">The raw JWT token string.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="header"/> or <paramref name="payload"/> is null.</exception>
        public JwtToken(IDictionary<string, object> header, IDictionary<string, object> payload, string rawToken)
        {
            Header = header ?? throw new ArgumentNullException(nameof(header));
            Payload = payload ?? throw new ArgumentNullException(nameof(payload));
            RawToken = rawToken ?? throw new ArgumentNullException(nameof(rawToken));
        }

        /// <summary>
        /// Gets the JWT header claims.
        /// </summary>
        public IDictionary<string, object> Header { get; }

        /// <summary>
        /// Gets the JWT payload claims.
        /// </summary>
        public IDictionary<string, object> Payload { get; }

        /// <summary>
        /// Gets the raw JWT token string.
        /// </summary>
        public string RawToken { get; }

        /// <summary>
        /// Gets the JWT signature.
        /// </summary>
        public string Signature
        {
            get
            {
                var parts = RawToken.Split('.');
                return parts.Length >= 3 ? parts[2] : string.Empty;
            }
        }

        /// <summary>
        /// Gets the JWT header claims as a read-only dictionary.
        /// </summary>
        IReadOnlyDictionary<string, object> IJwtToken.Header => new Dictionary<string, object>(Header);

        /// <summary>
        /// Gets the JWT payload claims as a read-only dictionary.
        /// </summary>
        IReadOnlyDictionary<string, object> IJwtToken.Payload => new Dictionary<string, object>(Payload);

        /// <summary>
        /// Gets a claim value from the JWT payload.
        /// </summary>
        /// <typeparam name="T">The type to convert the claim value to.</typeparam>
        /// <param name="claimName">The name of the claim.</param>
        /// <returns>The claim value converted to the specified type.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="claimName"/> is null or empty.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the claim is not found in the payload.</exception>
        /// <exception cref="InvalidCastException">Thrown when the claim value cannot be converted to the specified type.</exception>
        public T GetClaim<T>(string claimName)
        {
            if (string.IsNullOrEmpty(claimName))
            {
                throw new ArgumentNullException(nameof(claimName));
            }

            if (!Payload.TryGetValue(claimName, out var claimValue))
            {
                throw new KeyNotFoundException($"Claim '{claimName}' not found in JWT payload.");
            }

            if (claimValue is T typedValue)
            {
                return typedValue;
            }

            // Special handling for string conversion
            if (typeof(T) == typeof(string))
            {
                return (T)(object)(claimValue?.ToString() ?? string.Empty);
            }

            try
            {
                return (T)Convert.ChangeType(claimValue, typeof(T));
            }
            catch (Exception ex) when (ex is InvalidCastException || ex is FormatException || ex is OverflowException)
            {
                throw new InvalidCastException($"Cannot convert claim '{claimName}' value to type {typeof(T).Name}.", ex);
            }
        }

        /// <summary>
        /// Tries to get a claim value from the JWT payload.
        /// </summary>
        /// <typeparam name="T">The type to convert the claim value to.</typeparam>
        /// <param name="claimName">The name of the claim.</param>
        /// <param name="value">When this method returns, contains the claim value converted to the specified type, if the claim is found and conversion succeeds; otherwise, the default value for the type.</param>
        /// <returns>true if the claim is found and conversion succeeds; otherwise, false.</returns>
        public bool TryGetClaim<T>(string claimName, out T value)
        {
            value = default;

            if (string.IsNullOrEmpty(claimName) || !Payload.TryGetValue(claimName, out var claimValue))
            {
                return false;
            }

            if (claimValue is T typedValue)
            {
                value = typedValue;
                return true;
            }

            try
            {
                value = (T)Convert.ChangeType(claimValue, typeof(T));
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a header value from the JWT header.
        /// </summary>
        /// <typeparam name="T">The type to convert the header value to.</typeparam>
        /// <param name="headerName">The name of the header.</param>
        /// <returns>The header value converted to the specified type.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="headerName"/> is null or empty.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the header is not found in the JWT header.</exception>
        /// <exception cref="InvalidCastException">Thrown when the header value cannot be converted to the specified type.</exception>
        public T GetHeader<T>(string headerName)
        {
            if (string.IsNullOrEmpty(headerName))
            {
                throw new ArgumentNullException(nameof(headerName));
            }

            if (!Header.TryGetValue(headerName, out var headerValue))
            {
                throw new KeyNotFoundException($"Header '{headerName}' not found in JWT header.");
            }

            if (headerValue is T typedValue)
            {
                return typedValue;
            }

            try
            {
                return (T)Convert.ChangeType(headerValue, typeof(T));
            }
            catch (Exception ex) when (ex is InvalidCastException || ex is FormatException || ex is OverflowException)
            {
                throw new InvalidCastException($"Cannot convert header '{headerName}' value to type {typeof(T).Name}.", ex);
            }
        }

        /// <summary>
        /// Tries to get a header value from the JWT header.
        /// </summary>
        /// <typeparam name="T">The type to convert the header value to.</typeparam>
        /// <param name="headerName">The name of the header.</param>
        /// <param name="value">When this method returns, contains the header value converted to the specified type, if the header is found and conversion succeeds; otherwise, the default value for the type.</param>
        /// <returns>true if the header is found and conversion succeeds; otherwise, false.</returns>
        public bool TryGetHeader<T>(string headerName, out T value)
        {
            value = default;

            if (string.IsNullOrEmpty(headerName) || !Header.TryGetValue(headerName, out var headerValue))
            {
                return false;
            }

            if (headerValue is T typedValue)
            {
                value = typedValue;
                return true;
            }

            // Special case for string conversion, since JsonElement needs ToString() called
            if (typeof(T) == typeof(string))
            {
                value = (T)(object)(headerValue?.ToString() ?? string.Empty);
                return true;
            }

            try
            {
                value = (T)Convert.ChangeType(headerValue, typeof(T));
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Attempts to get a header claim by name. This is an alias for TryGetHeader to maintain compatibility with IJwtToken.
        /// </summary>
        /// <typeparam name="T">The expected type of the header claim value.</typeparam>
        /// <param name="claimName">The name of the header claim to retrieve.</param>
        /// <param name="value">When this method returns, contains the value of the header claim, if found; otherwise, the default value for the type of the value parameter.</param>
        /// <returns>true if the header claim is found; otherwise, false.</returns>
        bool IJwtToken.TryGetHeaderClaim<T>(string claimName, out T value)
        {
            return TryGetHeader(claimName, out value);
        }

        /// <summary>
        /// Creates a new JWT token with the specified claim added or updated.
        /// </summary>
        /// <param name="name">The name of the claim.</param>
        /// <param name="value">The value of the claim.</param>
        /// <returns>A new JWT token with the updated claim.</returns>
        public IJwtToken WithClaim(string name, object value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var newPayload = new Dictionary<string, object>(Payload);
            newPayload[name] = value;

            return new JwtToken(new Dictionary<string, object>(Header), newPayload, RawToken);
        }

        /// <summary>
        /// Creates a new JWT token with the specified claim removed.
        /// </summary>
        /// <param name="name">The name of the claim to remove.</param>
        /// <returns>A new JWT token without the specified claim.</returns>
        public IJwtToken WithoutClaim(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var newPayload = new Dictionary<string, object>(Payload);
            newPayload.Remove(name);

            return new JwtToken(new Dictionary<string, object>(Header), newPayload, RawToken);
        }

        /// <summary>
        /// Creates a new JWT token with the specified header claim added or updated.
        /// </summary>
        /// <param name="name">The name of the header claim.</param>
        /// <param name="value">The value of the header claim.</param>
        /// <returns>A new JWT token with the updated header claim.</returns>
        public IJwtToken WithHeaderClaim(string name, object value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var newHeader = new Dictionary<string, object>(Header);
            newHeader[name] = value;

            return new JwtToken(newHeader, new Dictionary<string, object>(Payload), RawToken);
        }

        /// <summary>
        /// Creates a new JWT token with the specified header claim removed.
        /// </summary>
        /// <param name="name">The name of the header claim to remove.</param>
        /// <returns>A new JWT token without the specified header claim.</returns>
        public IJwtToken WithoutHeaderClaim(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var newHeader = new Dictionary<string, object>(Header);
            newHeader.Remove(name);

            return new JwtToken(newHeader, new Dictionary<string, object>(Payload), RawToken);
        }
    }
} 