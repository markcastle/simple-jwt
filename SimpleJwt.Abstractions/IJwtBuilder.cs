using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace SimpleJwt.Abstractions
{
    /// <summary>
    /// Provides functionality to build JWT tokens.
    /// </summary>
    public interface IJwtBuilder
    {
        /// <summary>
        /// Adds a claim to the JWT payload.
        /// </summary>
        /// <param name="name">The name of the claim.</param>
        /// <param name="value">The value of the claim.</param>
        /// <returns>The current <see cref="IJwtBuilder"/> instance.</returns>
        IJwtBuilder AddClaim(string name, object value);

        /// <summary>
        /// Adds multiple claims to the JWT payload.
        /// </summary>
        /// <param name="claims">The claims to add.</param>
        /// <returns>The current <see cref="IJwtBuilder"/> instance.</returns>
        IJwtBuilder AddClaims(IDictionary<string, object> claims);

        /// <summary>
        /// Sets the JWT issuer claim.
        /// </summary>
        /// <param name="issuer">The issuer.</param>
        /// <returns>The current <see cref="IJwtBuilder"/> instance.</returns>
        IJwtBuilder SetIssuer(string issuer);

        /// <summary>
        /// Sets the JWT audience claim.
        /// </summary>
        /// <param name="audience">The audience.</param>
        /// <returns>The current <see cref="IJwtBuilder"/> instance.</returns>
        IJwtBuilder SetAudience(string audience);

        /// <summary>
        /// Sets the JWT expiration time claim.
        /// </summary>
        /// <param name="expirationTime">The expiration time.</param>
        /// <returns>The current <see cref="IJwtBuilder"/> instance.</returns>
        IJwtBuilder SetExpirationTime(DateTime expirationTime);

        /// <summary>
        /// Sets the JWT expiration time claim based on a TimeSpan from now.
        /// </summary>
        /// <param name="lifetime">The lifetime of the token.</param>
        /// <returns>The current <see cref="IJwtBuilder"/> instance.</returns>
        IJwtBuilder SetExpiration(TimeSpan lifetime);

        /// <summary>
        /// Sets the JWT not before claim.
        /// </summary>
        /// <param name="notBefore">The not before time.</param>
        /// <returns>The current <see cref="IJwtBuilder"/> instance.</returns>
        IJwtBuilder SetNotBefore(DateTime notBefore);

        /// <summary>
        /// Sets the JWT issued at claim.
        /// </summary>
        /// <param name="issuedAt">The issued at time.</param>
        /// <returns>The current <see cref="IJwtBuilder"/> instance.</returns>
        IJwtBuilder SetIssuedAt(DateTime issuedAt);

        /// <summary>
        /// Sets the JWT issued at claim to the current time.
        /// </summary>
        /// <returns>The current <see cref="IJwtBuilder"/> instance.</returns>
        IJwtBuilder SetIssuedNow();

        /// <summary>
        /// Sets the JWT ID claim.
        /// </summary>
        /// <param name="jwtId">The JWT ID.</param>
        /// <returns>The current <see cref="IJwtBuilder"/> instance.</returns>
        IJwtBuilder SetJwtId(string jwtId);

        /// <summary>
        /// Sets the JWT ID claim.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <returns>The current <see cref="IJwtBuilder"/> instance.</returns>
        IJwtBuilder SetId(string id);

        /// <summary>
        /// Sets the JWT subject claim.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <returns>The current <see cref="IJwtBuilder"/> instance.</returns>
        IJwtBuilder SetSubject(string subject);

        /// <summary>
        /// Adds a header claim to the JWT.
        /// </summary>
        /// <param name="name">The name of the header claim.</param>
        /// <param name="value">The value of the header claim.</param>
        /// <returns>The current <see cref="IJwtBuilder"/> instance.</returns>
        IJwtBuilder AddHeaderClaim(string name, object value);

        /// <summary>
        /// Sets the JWT key ID header claim.
        /// </summary>
        /// <param name="keyId">The key ID.</param>
        /// <returns>The current <see cref="IJwtBuilder"/> instance.</returns>
        IJwtBuilder SetKeyId(string keyId);

        /// <summary>
        /// Adds standard timestamp claims (iat, nbf, exp) based on a defined lifetime.
        /// </summary>
        /// <param name="lifetime">The lifetime of the token.</param>
        /// <returns>The current <see cref="IJwtBuilder"/> instance.</returns>
        IJwtBuilder AddLifetimeClaims(TimeSpan lifetime);

        /// <summary>
        /// Creates an unsigned JWT token.
        /// </summary>
        /// <returns>The unsigned JWT token string.</returns>
        string CreateUnsecured();

        /// <summary>
        /// Signs the JWT using HMAC-SHA256 algorithm.
        /// </summary>
        /// <param name="key">The key to use for signing.</param>
        /// <returns>The signed JWT token string.</returns>
        string SignHs256(byte[] key);

        /// <summary>
        /// Signs the JWT using HMAC-SHA256 algorithm.
        /// </summary>
        /// <param name="key">The key to use for signing.</param>
        /// <returns>The signed JWT token string.</returns>
        string SignHmacSha256(byte[] key);

        /// <summary>
        /// Signs the JWT using HMAC-SHA384 algorithm.
        /// </summary>
        /// <param name="key">The key to use for signing.</param>
        /// <returns>The signed JWT token string.</returns>
        string SignHs384(byte[] key);

        /// <summary>
        /// Signs the JWT using HMAC-SHA512 algorithm.
        /// </summary>
        /// <param name="key">The key to use for signing.</param>
        /// <returns>The signed JWT token string.</returns>
        string SignHs512(byte[] key);

        /// <summary>
        /// Signs the JWT using RSA-SHA256 algorithm.
        /// </summary>
        /// <param name="rsa">The RSA key to use for signing.</param>
        /// <returns>The signed JWT token string.</returns>
        string SignRs256(RSA rsa);

        /// <summary>
        /// Signs the JWT using RSA-SHA384 algorithm.
        /// </summary>
        /// <param name="rsa">The RSA key to use for signing.</param>
        /// <returns>The signed JWT token string.</returns>
        string SignRs384(RSA rsa);

        /// <summary>
        /// Signs the JWT using RSA-SHA512 algorithm.
        /// </summary>
        /// <param name="rsa">The RSA key to use for signing.</param>
        /// <returns>The signed JWT token string.</returns>
        string SignRs512(RSA rsa);

        /// <summary>
        /// Signs the JWT using ECDSA-SHA256 algorithm.
        /// </summary>
        /// <param name="ecdsa">The ECDSA key to use for signing.</param>
        /// <returns>The signed JWT token string.</returns>
        string SignEs256(ECDsa ecdsa);

        /// <summary>
        /// Signs the JWT using ECDSA-SHA384 algorithm.
        /// </summary>
        /// <param name="ecdsa">The ECDSA key to use for signing.</param>
        /// <returns>The signed JWT token string.</returns>
        string SignEs384(ECDsa ecdsa);

        /// <summary>
        /// Signs the JWT using ECDSA-SHA512 algorithm.
        /// </summary>
        /// <param name="ecdsa">The ECDSA key to use for signing.</param>
        /// <returns>The signed JWT token string.</returns>
        string SignEs512(ECDsa ecdsa);
    }
} 