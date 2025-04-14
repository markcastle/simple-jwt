using SimpleJwt.Abstractions;
using SimpleJwt.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using SimpleJwt.Abstractions.Serialization;

namespace SimpleJwt.Core
{
    /// <summary>
    /// Default implementation of <see cref="IJwtBuilder"/> for building JWT tokens.
    /// </summary>
    public class JwtBuilder : IJwtBuilder
    {
        private readonly Dictionary<string, object> _header;
        private readonly Dictionary<string, object> _payload;

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtBuilder"/> class.
        /// </summary>
        public JwtBuilder()
        {
            _header = new Dictionary<string, object>
            {
                { JwtConstants.HeaderType, JwtConstants.TokenTypeJwt }
            };

            _payload = new Dictionary<string, object>();
        }

        /// <summary>
        /// Adds a claim to the JWT payload.
        /// </summary>
        /// <param name="name">The name of the claim.</param>
        /// <param name="value">The value of the claim.</param>
        /// <returns>The current <see cref="IJwtBuilder"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null or empty.</exception>
        public IJwtBuilder AddClaim(string name, object value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            _payload[name] = value;
            return this;
        }

        /// <summary>
        /// Adds multiple claims to the JWT payload.
        /// </summary>
        /// <param name="claims">The claims to add.</param>
        /// <returns>The current <see cref="IJwtBuilder"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="claims"/> is null.</exception>
        public IJwtBuilder AddClaims(IDictionary<string, object> claims)
        {
            if (claims == null)
            {
                throw new ArgumentNullException(nameof(claims));
            }

            foreach (var claim in claims)
            {
                AddClaim(claim.Key, claim.Value);
            }

            return this;
        }

        /// <summary>
        /// Sets the issuer of the JWT token.
        /// </summary>
        /// <param name="issuer">The issuer of the token.</param>
        /// <returns>The current <see cref="IJwtBuilder"/> instance.</returns>
        public IJwtBuilder SetIssuer(string issuer)
        {
            return AddClaim(JwtConstants.ClaimIssuer, issuer);
        }

        /// <summary>
        /// Sets the subject of the JWT token.
        /// </summary>
        /// <param name="subject">The subject of the token.</param>
        /// <returns>The current <see cref="IJwtBuilder"/> instance.</returns>
        public IJwtBuilder SetSubject(string subject)
        {
            return AddClaim(JwtConstants.ClaimSubject, subject);
        }

        /// <summary>
        /// Sets the audience of the JWT token.
        /// </summary>
        /// <param name="audience">The audience of the token.</param>
        /// <returns>The current <see cref="IJwtBuilder"/> instance.</returns>
        public IJwtBuilder SetAudience(string audience)
        {
            return AddClaim(JwtConstants.ClaimAudience, audience);
        }

        /// <summary>
        /// Sets the JWT ID of the token.
        /// </summary>
        /// <param name="id">The JWT ID of the token.</param>
        /// <returns>The current <see cref="IJwtBuilder"/> instance.</returns>
        public IJwtBuilder SetId(string id)
        {
            return AddClaim(JwtConstants.ClaimJwtId, id);
        }

        /// <summary>
        /// Sets the JWT ID of the token.
        /// </summary>
        /// <param name="jwtId">The JWT ID of the token.</param>
        /// <returns>The current <see cref="IJwtBuilder"/> instance.</returns>
        public IJwtBuilder SetJwtId(string jwtId)
        {
            return AddClaim(JwtConstants.ClaimJwtId, jwtId);
        }

        /// <summary>
        /// Adds a header claim to the JWT.
        /// </summary>
        /// <param name="name">The name of the header claim.</param>
        /// <param name="value">The value of the header claim.</param>
        /// <returns>The current <see cref="IJwtBuilder"/> instance.</returns>
        public IJwtBuilder AddHeaderClaim(string name, object value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            _header[name] = value;
            return this;
        }

        /// <summary>
        /// Sets the JWT key ID header claim.
        /// </summary>
        /// <param name="keyId">The key ID.</param>
        /// <returns>The current <see cref="IJwtBuilder"/> instance.</returns>
        public IJwtBuilder SetKeyId(string keyId)
        {
            return AddHeaderClaim(JwtConstants.HeaderKeyId, keyId);
        }

        /// <summary>
        /// Sets the expiration time of the JWT token.
        /// </summary>
        /// <param name="expirationTime">The expiration time of the token.</param>
        /// <returns>The current <see cref="IJwtBuilder"/> instance.</returns>
        public IJwtBuilder SetExpirationTime(DateTime expirationTime)
        {
            long unixTimestamp = new DateTimeOffset(expirationTime).ToUnixTimeSeconds();
            return AddClaim(JwtConstants.ClaimExpirationTime, unixTimestamp);
        }

        /// <summary>
        /// Sets the expiration time of the JWT token.
        /// </summary>
        /// <param name="expirationTime">The expiration time of the token.</param>
        /// <returns>The current <see cref="IJwtBuilder"/> instance.</returns>
        public IJwtBuilder SetExpiration(DateTime expirationTime)
        {
            return SetExpirationTime(expirationTime);
        }

        /// <summary>
        /// Sets the expiration time of the JWT token relative to the current time.
        /// </summary>
        /// <param name="lifetime">The lifetime of the token.</param>
        /// <returns>The current <see cref="IJwtBuilder"/> instance.</returns>
        public IJwtBuilder SetExpiration(TimeSpan lifetime)
        {
            DateTime expirationTime = DateTime.UtcNow.Add(lifetime);
            return SetExpirationTime(expirationTime);
        }

        /// <summary>
        /// Adds standard timestamp claims (iat, nbf, exp) based on a defined lifetime.
        /// </summary>
        /// <param name="lifetime">The lifetime of the token.</param>
        /// <returns>The current <see cref="IJwtBuilder"/> instance.</returns>
        public IJwtBuilder AddLifetimeClaims(TimeSpan lifetime)
        {
            DateTime now = DateTime.UtcNow;
            SetIssuedAt(now);
            SetNotBefore(now);
            return SetExpirationTime(now.Add(lifetime));
        }

        /// <summary>
        /// Sets the time before which the JWT token is not valid.
        /// </summary>
        /// <param name="notBefore">The time before which the token is not valid.</param>
        /// <returns>The current <see cref="IJwtBuilder"/> instance.</returns>
        public IJwtBuilder SetNotBefore(DateTime notBefore)
        {
            long unixTimestamp = new DateTimeOffset(notBefore).ToUnixTimeSeconds();
            return AddClaim(JwtConstants.ClaimNotBefore, unixTimestamp);
        }

        /// <summary>
        /// Sets the time when the JWT token was issued.
        /// </summary>
        /// <param name="issuedAt">The time when the token was issued.</param>
        /// <returns>The current <see cref="IJwtBuilder"/> instance.</returns>
        public IJwtBuilder SetIssuedAt(DateTime issuedAt)
        {
            long unixTimestamp = new DateTimeOffset(issuedAt).ToUnixTimeSeconds();
            return AddClaim(JwtConstants.ClaimIssuedAt, unixTimestamp);
        }

        /// <summary>
        /// Sets the current time as the time when the JWT token was issued.
        /// </summary>
        /// <returns>The current <see cref="IJwtBuilder"/> instance.</returns>
        public IJwtBuilder SetIssuedNow()
        {
            return SetIssuedAt(DateTime.UtcNow);
        }

        /// <summary>
        /// Signs the JWT token using the HMAC-SHA256 algorithm with the specified key.
        /// </summary>
        /// <param name="key">The key to use for signing the token.</param>
        /// <returns>The signed JWT token.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is empty.</exception>
        public string SignHs256(byte[] key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (key.Length == 0)
            {
                throw new ArgumentException("Key cannot be empty.", nameof(key));
            }

            _header[JwtConstants.HeaderAlgorithm] = JwtConstants.AlgorithmHs256;
            
            using (var hmac = new HMACSHA256(key))
            {
                return SignWithAlgorithm(hmac);
            }
        }

        /// <summary>
        /// Signs the JWT token using the HMAC-SHA256 algorithm with the specified key.
        /// </summary>
        /// <param name="key">The key to use for signing the token.</param>
        /// <returns>The signed JWT token.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is empty.</exception>
        public string SignHmacSha256(byte[] key)
        {
            return SignHs256(key);
        }

        /// <summary>
        /// Signs the JWT token using the HMAC-SHA384 algorithm with the specified key.
        /// </summary>
        /// <param name="key">The key to use for signing the token.</param>
        /// <returns>The signed JWT token.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is empty.</exception>
        public string SignHs384(byte[] key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (key.Length == 0)
            {
                throw new ArgumentException("Key cannot be empty.", nameof(key));
            }

            _header[JwtConstants.HeaderAlgorithm] = JwtConstants.AlgorithmHs384;
            
            using (var hmac = new HMACSHA384(key))
            {
                return SignWithAlgorithm(hmac);
            }
        }

        /// <summary>
        /// Signs the JWT token using the HMAC-SHA512 algorithm with the specified key.
        /// </summary>
        /// <param name="key">The key to use for signing the token.</param>
        /// <returns>The signed JWT token.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="key"/> is empty.</exception>
        public string SignHs512(byte[] key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (key.Length == 0)
            {
                throw new ArgumentException("Key cannot be empty.", nameof(key));
            }

            _header[JwtConstants.HeaderAlgorithm] = JwtConstants.AlgorithmHs512;
            
            using (var hmac = new HMACSHA512(key))
            {
                return SignWithAlgorithm(hmac);
            }
        }

        /// <summary>
        /// Signs the JWT token using the specified HMAC algorithm.
        /// </summary>
        /// <param name="algorithm">The HMAC algorithm to use for signing.</param>
        /// <returns>The signed JWT token.</returns>
        private string SignWithAlgorithm(HMAC algorithm)
        {
            string headerJson = JsonProviderConfiguration.GetProvider().Serialize(_header);
            string payloadJson = JsonProviderConfiguration.GetProvider().Serialize(_payload);

            string headerBase64 = JwtBase64UrlEncoder.Encode(headerJson);
            string payloadBase64 = JwtBase64UrlEncoder.Encode(payloadJson);

            string data = $"{headerBase64}.{payloadBase64}";
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] signature = algorithm.ComputeHash(dataBytes);
            
            string signatureBase64 = JwtBase64UrlEncoder.Encode(signature);

            return $"{data}.{signatureBase64}";
        }

        /// <summary>
        /// Creates an unsigned JWT token.
        /// </summary>
        /// <returns>The unsigned JWT token.</returns>
        public string CreateUnsecured()
        {
            _header[JwtConstants.HeaderAlgorithm] = JwtConstants.AlgorithmNone;

            string headerJson = JsonProviderConfiguration.GetProvider().Serialize(_header);
            string payloadJson = JsonProviderConfiguration.GetProvider().Serialize(_payload);

            string headerBase64 = JwtBase64UrlEncoder.Encode(headerJson);
            string payloadBase64 = JwtBase64UrlEncoder.Encode(payloadJson);

            return $"{headerBase64}.{payloadBase64}.";
        }

        /// <summary>
        /// Signs the JWT using RSA-SHA256 algorithm.
        /// </summary>
        /// <param name="rsa">The RSA key to use for signing.</param>
        /// <returns>The signed JWT token string.</returns>
        public string SignRs256(RSA rsa)
        {
            if (rsa == null)
            {
                throw new ArgumentNullException(nameof(rsa));
            }

            _header[JwtConstants.HeaderAlgorithm] = JwtConstants.AlgorithmRs256;
            return SignWithRsa(rsa, HashAlgorithmName.SHA256);
        }

        /// <summary>
        /// Signs the JWT using RSA-SHA384 algorithm.
        /// </summary>
        /// <param name="rsa">The RSA key to use for signing.</param>
        /// <returns>The signed JWT token string.</returns>
        public string SignRs384(RSA rsa)
        {
            if (rsa == null)
            {
                throw new ArgumentNullException(nameof(rsa));
            }

            _header[JwtConstants.HeaderAlgorithm] = JwtConstants.AlgorithmRs384;
            return SignWithRsa(rsa, HashAlgorithmName.SHA384);
        }

        /// <summary>
        /// Signs the JWT using RSA-SHA512 algorithm.
        /// </summary>
        /// <param name="rsa">The RSA key to use for signing.</param>
        /// <returns>The signed JWT token string.</returns>
        public string SignRs512(RSA rsa)
        {
            if (rsa == null)
            {
                throw new ArgumentNullException(nameof(rsa));
            }

            _header[JwtConstants.HeaderAlgorithm] = JwtConstants.AlgorithmRs512;
            return SignWithRsa(rsa, HashAlgorithmName.SHA512);
        }

        private string SignWithRsa(RSA rsa, HashAlgorithmName hashAlgorithm)
        {
            string headerJson = JsonProviderConfiguration.GetProvider().Serialize(_header);
            string payloadJson = JsonProviderConfiguration.GetProvider().Serialize(_payload);

            string headerBase64 = JwtBase64UrlEncoder.Encode(headerJson);
            string payloadBase64 = JwtBase64UrlEncoder.Encode(payloadJson);

            string data = $"{headerBase64}.{payloadBase64}";
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            
            byte[] signature = rsa.SignData(dataBytes, hashAlgorithm, RSASignaturePadding.Pkcs1);
            string signatureBase64 = JwtBase64UrlEncoder.Encode(signature);

            return $"{data}.{signatureBase64}";
        }

        /// <summary>
        /// Signs the JWT using ECDSA-SHA256 algorithm.
        /// </summary>
        /// <param name="ecdsa">The ECDSA key to use for signing.</param>
        /// <returns>The signed JWT token string.</returns>
        public string SignEs256(ECDsa ecdsa)
        {
            if (ecdsa == null)
            {
                throw new ArgumentNullException(nameof(ecdsa));
            }

            _header[JwtConstants.HeaderAlgorithm] = JwtConstants.AlgorithmEs256;
            return SignWithEcdsa(ecdsa, HashAlgorithmName.SHA256);
        }

        /// <summary>
        /// Signs the JWT using ECDSA-SHA384 algorithm.
        /// </summary>
        /// <param name="ecdsa">The ECDSA key to use for signing.</param>
        /// <returns>The signed JWT token string.</returns>
        public string SignEs384(ECDsa ecdsa)
        {
            if (ecdsa == null)
            {
                throw new ArgumentNullException(nameof(ecdsa));
            }

            _header[JwtConstants.HeaderAlgorithm] = JwtConstants.AlgorithmEs384;
            return SignWithEcdsa(ecdsa, HashAlgorithmName.SHA384);
        }

        /// <summary>
        /// Signs the JWT using ECDSA-SHA512 algorithm.
        /// </summary>
        /// <param name="ecdsa">The ECDSA key to use for signing.</param>
        /// <returns>The signed JWT token string.</returns>
        public string SignEs512(ECDsa ecdsa)
        {
            if (ecdsa == null)
            {
                throw new ArgumentNullException(nameof(ecdsa));
            }

            _header[JwtConstants.HeaderAlgorithm] = JwtConstants.AlgorithmEs512;
            return SignWithEcdsa(ecdsa, HashAlgorithmName.SHA512);
        }

        private string SignWithEcdsa(ECDsa ecdsa, HashAlgorithmName hashAlgorithm)
        {
            string headerJson = JsonProviderConfiguration.GetProvider().Serialize(_header);
            string payloadJson = JsonProviderConfiguration.GetProvider().Serialize(_payload);

            string headerBase64 = JwtBase64UrlEncoder.Encode(headerJson);
            string payloadBase64 = JwtBase64UrlEncoder.Encode(payloadJson);

            string data = $"{headerBase64}.{payloadBase64}";
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            
            byte[] signature = ecdsa.SignData(dataBytes, hashAlgorithm);
            string signatureBase64 = JwtBase64UrlEncoder.Encode(signature);

            return $"{data}.{signatureBase64}";
        }

        /// <summary>
        /// Sets a header parameter in the JWT.
        /// </summary>
        /// <param name="name">The name of the header parameter.</param>
        /// <param name="value">The value of the header parameter.</param>
        /// <returns>The current <see cref="IJwtBuilder"/> instance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null or empty.</exception>
        public IJwtBuilder SetHeaderParameter(string name, object value)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            _header[name] = value;
            return this;
        }
    }
} 