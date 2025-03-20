namespace SimpleJwt.Abstractions
{
    /// <summary>
    /// Contains constant values for JWT-related operations.
    /// </summary>
    public static class JwtConstants
    {
        /// <summary>
        /// The JWT header parameter for the token type.
        /// </summary>
        public const string HeaderType = "typ";

        /// <summary>
        /// The JWT header parameter for the algorithm.
        /// </summary>
        public const string HeaderAlgorithm = "alg";

        /// <summary>
        /// The JWT header parameter for the key ID.
        /// </summary>
        public const string HeaderKeyId = "kid";

        /// <summary>
        /// The JWT claim for the issuer.
        /// </summary>
        public const string ClaimIssuer = "iss";

        /// <summary>
        /// The JWT claim for the subject.
        /// </summary>
        public const string ClaimSubject = "sub";

        /// <summary>
        /// The JWT claim for the audience.
        /// </summary>
        public const string ClaimAudience = "aud";

        /// <summary>
        /// The JWT claim for the expiration time.
        /// </summary>
        public const string ClaimExpirationTime = "exp";

        /// <summary>
        /// The JWT claim for the not before time.
        /// </summary>
        public const string ClaimNotBefore = "nbf";

        /// <summary>
        /// The JWT claim for the issued at time.
        /// </summary>
        public const string ClaimIssuedAt = "iat";

        /// <summary>
        /// The JWT claim for the JWT ID.
        /// </summary>
        public const string ClaimJwtId = "jti";

        /// <summary>
        /// The JWT type value for the JWT token type.
        /// </summary>
        public const string TokenTypeJwt = "JWT";

        /// <summary>
        /// The HMAC SHA-256 algorithm identifier.
        /// </summary>
        public const string AlgorithmHs256 = "HS256";

        /// <summary>
        /// The HMAC SHA-384 algorithm identifier.
        /// </summary>
        public const string AlgorithmHs384 = "HS384";

        /// <summary>
        /// The HMAC SHA-512 algorithm identifier.
        /// </summary>
        public const string AlgorithmHs512 = "HS512";

        /// <summary>
        /// The RSA SHA-256 algorithm identifier.
        /// </summary>
        public const string AlgorithmRs256 = "RS256";

        /// <summary>
        /// The RSA SHA-384 algorithm identifier.
        /// </summary>
        public const string AlgorithmRs384 = "RS384";

        /// <summary>
        /// The RSA SHA-512 algorithm identifier.
        /// </summary>
        public const string AlgorithmRs512 = "RS512";

        /// <summary>
        /// The ECDSA SHA-256 algorithm identifier.
        /// </summary>
        public const string AlgorithmEs256 = "ES256";

        /// <summary>
        /// The ECDSA SHA-384 algorithm identifier.
        /// </summary>
        public const string AlgorithmEs384 = "ES384";

        /// <summary>
        /// The ECDSA SHA-512 algorithm identifier.
        /// </summary>
        public const string AlgorithmEs512 = "ES512";

        /// <summary>
        /// The "none" algorithm identifier.
        /// </summary>
        public const string AlgorithmNone = "none";
    }
} 