namespace SimpleJwt.Abstractions.Validation
{
    /// <summary>
    /// Contains constant values for validation error codes.
    /// </summary>
    public static class ValidationCodes
    {
        /// <summary>
        /// The token is invalid or malformed.
        /// </summary>
        public const string InvalidToken = "invalid_token";

        /// <summary>
        /// The token has expired.
        /// </summary>
        public const string TokenExpired = "token_expired";

        /// <summary>
        /// The token is not yet valid (used when nbf claim is in the future).
        /// </summary>
        public const string TokenNotYetValid = "token_not_yet_valid";

        /// <summary>
        /// The token's issuer is invalid.
        /// </summary>
        public const string InvalidIssuer = "invalid_issuer";

        /// <summary>
        /// The token's audience is invalid.
        /// </summary>
        public const string InvalidAudience = "invalid_audience";

        /// <summary>
        /// The token's signature is invalid.
        /// </summary>
        public const string InvalidSignature = "invalid_signature";

        /// <summary>
        /// A required claim is missing from the token.
        /// </summary>
        public const string MissingClaim = "missing_claim";

        /// <summary>
        /// A claim in the token has an invalid value.
        /// </summary>
        public const string InvalidClaimValue = "invalid_claim_value";
    }
} 