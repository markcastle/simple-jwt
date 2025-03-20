using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace SimpleJwt.Abstractions.Validation
{
    /// <summary>
    /// Represents the parameters used to validate a JWT token.
    /// </summary>
    public class ValidationParameters
    {
        /// <summary>
        /// Gets or sets the issuer that the token is allowed to come from.
        /// </summary>
        public string ValidIssuer { get; set; }

        /// <summary>
        /// Gets or sets a collection of valid issuers that the token is allowed to come from.
        /// </summary>
        public ICollection<string> ValidIssuers { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the audience that the token is intended for.
        /// </summary>
        public string ValidAudience { get; set; }

        /// <summary>
        /// Gets or sets a collection of valid audiences that the token is intended for.
        /// </summary>
        public ICollection<string> ValidAudiences { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the clock skew to apply when validating the token's lifetime.
        /// </summary>
        public TimeSpan ClockSkew { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Gets or sets a value indicating whether to validate the token's lifetime (nbf and exp claims).
        /// </summary>
        public bool ValidateLifetime { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to validate the token's issuer.
        /// </summary>
        public bool ValidateIssuer { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to validate the token's audience.
        /// </summary>
        public bool ValidateAudience { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to validate the token's signature.
        /// </summary>
        public bool ValidateSignature { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to validate the token's ID (jti claim) to prevent replay attacks.
        /// </summary>
        public bool ValidateJti { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether to save validated tokens to a cache to improve performance.
        /// </summary>
        public bool EnableCaching { get; set; } = false;

        /// <summary>
        /// Gets or sets the duration for which validation results should be cached.
        /// </summary>
        public TimeSpan CacheDuration { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Gets or sets the symmetric security key used to validate the token's signature.
        /// </summary>
        public byte[] SymmetricSecurityKey { get; set; }

        /// <summary>
        /// Gets or sets the RSA security key used to validate the token's signature.
        /// </summary>
        public RSA RsaSecurityKey { get; set; }

        /// <summary>
        /// Gets or sets the ECDSA security key used to validate the token's signature.
        /// </summary>
        public ECDsa EcdsaSecurityKey { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of key IDs to security keys for key rotation support.
        /// </summary>
        public IDictionary<string, object> SecurityKeys { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets a delegate to validate the token ID (jti) to prevent replay attacks.
        /// </summary>
        public Func<string, bool> JtiValidator { get; set; }

        /// <summary>
        /// Gets or sets a delegate to validate the token's confirmation method (cnf) claim.
        /// </summary>
        public Func<IDictionary<string, object>, bool> ConfirmationMethodValidator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable sliding expiration, which extends the token's lifetime on each validation.
        /// </summary>
        public bool EnableSlidingExpiration { get; set; } = false;

        /// <summary>
        /// Gets or sets the duration by which to extend the token's lifetime when sliding expiration is enabled.
        /// </summary>
        public TimeSpan SlidingExpirationDuration { get; set; } = TimeSpan.FromMinutes(30);
        
        /// <summary>
        /// Gets or sets the maximum allowed token size in bytes to prevent DOS attacks.
        /// </summary>
        public int MaximumTokenSizeInBytes { get; set; } = 8192;
        
        /// <summary>
        /// Gets or sets a value indicating whether to require the 'typ' header to be present.
        /// </summary>
        public bool RequireTokenType { get; set; } = false;
        
        /// <summary>
        /// Gets or sets the required token type in the 'typ' header.
        /// </summary>
        public string RequiredTokenType { get; set; } = "JWT";
        
        /// <summary>
        /// Gets or sets a delegate for custom token validation.
        /// </summary>
        public Func<IJwtToken, ValidationResult> CustomValidator { get; set; }
    }
} 