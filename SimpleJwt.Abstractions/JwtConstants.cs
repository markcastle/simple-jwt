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

        /// <summary>
        /// The OpenID Connect claim for the full name.
        /// </summary>
        public const string ClaimName = "name";

        /// <summary>
        /// The OpenID Connect claim for the given name.
        /// </summary>
        public const string ClaimGivenName = "given_name";

        /// <summary>
        /// The OpenID Connect claim for the family name.
        /// </summary>
        public const string ClaimFamilyName = "family_name";

        /// <summary>
        /// The OpenID Connect claim for the middle name.
        /// </summary>
        public const string ClaimMiddleName = "middle_name";

        /// <summary>
        /// The OpenID Connect claim for the nickname.
        /// </summary>
        public const string ClaimNickname = "nickname";

        /// <summary>
        /// The OpenID Connect claim for the preferred username.
        /// </summary>
        public const string ClaimPreferredUsername = "preferred_username";

        /// <summary>
        /// The OpenID Connect claim for the profile URL.
        /// </summary>
        public const string ClaimProfile = "profile";

        /// <summary>
        /// The OpenID Connect claim for the picture URL.
        /// </summary>
        public const string ClaimPicture = "picture";

        /// <summary>
        /// The OpenID Connect claim for the website URL.
        /// </summary>
        public const string ClaimWebsite = "website";

        /// <summary>
        /// The OpenID Connect claim for the email address.
        /// </summary>
        public const string ClaimEmail = "email";

        /// <summary>
        /// The OpenID Connect claim for the email verification status.
        /// </summary>
        public const string ClaimEmailVerified = "email_verified";

        /// <summary>
        /// The OpenID Connect claim for the gender.
        /// </summary>
        public const string ClaimGender = "gender";

        /// <summary>
        /// The OpenID Connect claim for the birthdate.
        /// </summary>
        public const string ClaimBirthdate = "birthdate";

        /// <summary>
        /// The OpenID Connect claim for the timezone.
        /// </summary>
        public const string ClaimZoneinfo = "zoneinfo";

        /// <summary>
        /// The OpenID Connect claim for the locale.
        /// </summary>
        public const string ClaimLocale = "locale";

        /// <summary>
        /// The OpenID Connect claim for the phone number.
        /// </summary>
        public const string ClaimPhoneNumber = "phone_number";

        /// <summary>
        /// The OpenID Connect claim for the phone number verification status.
        /// </summary>
        public const string ClaimPhoneNumberVerified = "phone_number_verified";

        /// <summary>
        /// The OpenID Connect claim for the address.
        /// </summary>
        public const string ClaimAddress = "address";

        /// <summary>
        /// The OpenID Connect claim for the last update time.
        /// </summary>
        public const string ClaimUpdatedAt = "updated_at";

        /// <summary>
        /// The "azp" (authorized party) claim name.
        /// </summary>
        public const string ClaimAuthorizedParty = "azp";

        /// <summary>
        /// The "auth_time" (authentication time) claim name.
        /// </summary>
        public const string ClaimAuthenticationTime = "auth_time";

        /// <summary>
        /// The "nonce" claim name.
        /// </summary>
        public const string ClaimNonce = "nonce";

        /// <summary>
        /// The "acr" (authentication context class reference) claim name.
        /// </summary>
        public const string ClaimAuthenticationContextClassReference = "acr";

        /// <summary>
        /// The "amr" (authentication methods references) claim name.
        /// </summary>
        public const string ClaimAuthenticationMethodsReferences = "amr";

        /// <summary>
        /// The "at_hash" (access token hash) claim name.
        /// </summary>
        public const string ClaimAccessTokenHash = "at_hash";

        /// <summary>
        /// The "c_hash" (code hash) claim name.
        /// </summary>
        public const string ClaimCodeHash = "c_hash";

        /// <summary>
        /// The "s_hash" (state hash) claim name.
        /// </summary>
        public const string ClaimStateHash = "s_hash";

        /// <summary>
        /// The "sid" (session ID) claim name.
        /// </summary>
        public const string ClaimSessionId = "sid";

        /// <summary>
        /// The "scope" claim name.
        /// </summary>
        public const string ClaimScope = "scope";

        /// <summary>
        /// The "client_id" claim name.
        /// </summary>
        public const string ClaimClientId = "client_id";

        /// <summary>
        /// The "username" claim name.
        /// </summary>
        public const string ClaimUsername = "username";
    }
} 