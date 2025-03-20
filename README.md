# SimpleJwt

A lightweight, secure, and extensible JWT (JSON Web Token) library for .NET applications.

## Features

- **Standards Compliant**: Fully compliant with RFC 7519 (JWT), RFC 7515 (JWS), and related specifications
- **Immutable Token Design**: Thread-safe token processing with functional-style modification methods
- **Comprehensive Validation**: Extensive validation options for security-critical applications
- **Async Support**: First-class async API for high-performance scenarios
- **Enhanced Security Features**:
  - JWT Confirmation Method (cnf) claim validation
  - JWT ID (jti) tracking to prevent replay attacks
  - Token type validation to prevent substitution attacks
  - Key rotation support
- **Token Lifetime Management**:
  - Token refresh and sliding expiration mechanisms
  - Token revocation capabilities
- **Performance Optimized**:
  - Caching layer for parsed tokens and validation results
  - Efficient token parsing and validation

## Installation

```bash
dotnet add package SimpleJwt.Core
```

## Quick Start

### Creating and Signing a JWT

```csharp
// Create a JWT with standard claims
string token = JwtBuilder.Create()
    .SetIssuer("https://myissuer.com")
    .SetAudience("https://myapi.com")
    .SetSubject("user123")
    .AddClaim("role", "admin")
    .AddLifetimeClaims(TimeSpan.FromHours(1))
    .SignHs256(Encoding.UTF8.GetBytes("your-secret-key-with-at-least-32-bytes"));
```

### Parsing and Validating a JWT

```csharp
// Parse the token
IJwtParser parser = new JwtParser();
IJwtToken parsedToken = parser.Parse(token);

// Set up validation parameters
var validationParameters = new ValidationParameters
{
    ValidIssuer = "https://myissuer.com",
    ValidAudience = "https://myapi.com",
    SymmetricSecurityKey = Encoding.UTF8.GetBytes("your-secret-key-with-at-least-32-bytes")
};

// Validate the token
IJwtValidator validator = new JwtValidator();
ValidationResult result = validator.Validate(parsedToken, validationParameters);

if (result.IsValid)
{
    // Token is valid, use its claims
    if (parsedToken.TryGetClaim<string>("role", out var role))
    {
        Console.WriteLine($"User has role: {role}");
    }
}
else
{
    // Token is invalid, handle errors
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"Validation error: {error}");
    }
}
```

### Async Token Validation

```csharp
// Asynchronously parse and validate a token
IJwtToken parsedToken = await parser.ParseAsync(token);
ValidationResult result = await validator.ValidateAsync(parsedToken, validationParameters);
```

### Token Refresh

```csharp
// Create a refresh token for an access token
ITokenRefresher refresher = new TokenRefresher();
string refreshToken = refresher.CreateRefreshToken(accessToken, TimeSpan.FromDays(30));

// Later, use the refresh token to get a new access token
RefreshResult refreshResult = await refresher.RefreshAsync(accessToken, refreshToken);

if (refreshResult.IsSuccess)
{
    string newAccessToken = refreshResult.AccessToken;
    string newRefreshToken = refreshResult.RefreshToken;
}
```

### Token Revocation

```csharp
// Revoke a token
ITokenRevoker revoker = new TokenRevoker();
bool revoked = await revoker.RevokeAsync(token, "User logged out");

// Check if a token is revoked before using it
if (await revoker.IsRevokedAsync(token))
{
    // Token is revoked, reject the request
}
```

## Advanced Features

### Custom Claims Transformation

```csharp
// Implement the IClaimsTransformer interface
public class MyClaimsTransformer : IClaimsTransformer
{
    public IDictionary<string, object> TransformClaims(
        IDictionary<string, object> claims, 
        ValidationContext context)
    {
        // Transform claims (e.g., add, modify, or remove claims)
        var transformedClaims = new Dictionary<string, object>(claims);
        transformedClaims["customClaim"] = "customValue";
        return transformedClaims;
    }

    public Task<IDictionary<string, object>> TransformClaimsAsync(
        IDictionary<string, object> claims, 
        ValidationContext context,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(TransformClaims(claims, context));
    }
}
```

### Working with Keys

```csharp
// Using RSA for JWT signing and validation
using var rsa = RSA.Create(2048);
string token = JwtBuilder.Create()
    .SetIssuer("https://myissuer.com")
    .AddClaim("customClaim", "customValue")
    .SignRs256(rsa);

var validationParameters = new ValidationParameters
{
    ValidIssuer = "https://myissuer.com",
    RsaSecurityKey = rsa
};
```

### Token Caching

```csharp
// Create a cache provider
ITokenCache cache = new InMemoryTokenCache();

// Configure the JWT validator to use caching
var validationParameters = new ValidationParameters
{
    EnableCaching = true,
    CacheDuration = TimeSpan.FromMinutes(5)
};

// Create a validator that uses the cache
var validator = new JwtValidator(cache);
```

## Security Best Practices

- Use strong keys (at least 256 bits for HMAC, 2048 bits for RSA)
- Always validate token lifetime (exp/nbf claims)
- Validate issuer and audience to prevent token reuse across systems
- Consider implementing JTI tracking to prevent replay attacks
- Use HTTPS for all token transmissions
- Set appropriate token lifetimes based on security requirements
- Implement token revocation for sensitive systems

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the LICENSE file for details. 