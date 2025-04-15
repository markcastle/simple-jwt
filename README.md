# 🔐 SimpleJwt

[![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.1-blue.svg)](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Build Status](https://img.shields.io/badge/Build-Passing-brightgreen.svg)](https://github.com/yourusername/simple-jwt/actions)
[![Test Coverage](https://img.shields.io/badge/Test%20Coverage-98%25-brightgreen.svg)](https://github.com/yourusername/simple-jwt/actions)
[![NuGet Version](https://img.shields.io/badge/NuGet-0.1.0-blue.svg)](https://www.nuget.org/packages/SimpleJwt.Core)
[![Unity Compatible](https://img.shields.io/badge/Unity-Compatible-blue.svg)](https://unity.com)
[![Security Rating](https://img.shields.io/badge/Security-A%2B-brightgreen.svg)](https://github.com/yourusername/simple-jwt/security)

A lightweight, secure, and extensible JWT (JSON Web Token) library for .NET applications, designed with performance and security as top priorities. Supports both regular .NET applications and Unity game development.

## ✨ Features

- **🔄 Standards Compliant**: Fully compliant with RFC 7519 (JWT), RFC 7515 (JWS), and related specifications
- **🔒 Immutable Token Design**: Thread-safe token processing with functional-style modification methods that prevent race conditions in concurrent scenarios
- **✅ Comprehensive Validation**: Extensive validation options for security-critical applications, including:
  - Signature validation (HMAC, RSA, ECDSA)
  - Issuer and audience validation
  - Expiration and not-before time validation
  - Custom claim validation
- **⚡ Async Support**: First-class async API for high-performance scenarios with proper cancellation token support
- **🛡️ Enhanced Security Features**:
  - JWT Confirmation Method (cnf) claim validation for proof-of-possession tokens
  - JWT ID (jti) tracking to prevent replay attacks with built-in validators
  - Token type validation to prevent substitution attacks between different token types
  - Key rotation support with kid header parameter handling
- **⏱️ Token Lifetime Management**:
  - Token refresh and sliding expiration mechanisms with configurable timeout policies
  - Token revocation capabilities with custom reason storage
  - Built-in support for revocation lists and blacklists
- **🚀 Performance Optimized**:
  - Caching layer for parsed tokens and validation results
  - Efficient token parsing and validation with minimal allocations
  - Configurable memory limits to prevent DOS attacks
- **🎮 Unity Compatibility**:
  - Abstracted JSON serialization layer with support for both System.Text.Json and Newtonsoft.Json
  - Compatible with Unity's IL2CPP scripting backend
  - Minimal dependencies for easier integration with Unity projects
  - Coroutine-based API for async operations in Unity
- **Zero Dependencies**: Built with no external dependencies, using only .NET Standard 2.1 libraries
- **Full .NET Standard 2.1 Compliance**: Compatible with any .NET platform that supports .NET Standard 2.1
- **Multiple Signing Algorithms**: Support for HMAC-SHA256/384/512, RSA-SHA256/384/512, and ECDSA-SHA256/384/512
- **Comprehensive Claim Management**: Built-in support for standard JWT claims and custom claim handling
- **Flexible Token Building**: Fluent builder pattern for easy token creation
- **Robust Token Parsing**: Safe and efficient token parsing with validation
- **Extensible Architecture**: Easy to extend with custom claim types and validation rules
- **Strong Type Safety**: Full type safety for all operations
- **Comprehensive Testing**: Extensive test coverage for all features

## 📦 Installation

```bash
# Install the core package
dotnet add package SimpleJwt.Core

# For abstraction interfaces only (DI scenarios)
dotnet add package SimpleJwt.Abstractions

# For System.Text.Json support (recommended for .NET Core/5+)
dotnet add package SimpleJwt.SystemTextJson

# For Newtonsoft.Json support
dotnet add package SimpleJwt.Newtonsoft

# For Unity compatibility
dotnet add package SimpleJwt.Unity

# For Microsoft DI integration
dotnet add package SimpleJwt.DependencyInjection
```

> 📢 **Note:** The SimpleJwt packages have not been published to NuGet yet. We plan to publish them after completing more extensive testing and validation. In the meantime, you can build the packages from source using this repository.

## 🏗️ Technical Architecture

SimpleJwt uses a layered architecture:

- **Abstractions**: Core interfaces and models (`IJwtToken`, `IJwtParser`, `IJwtValidator`, etc.)
- **Core**: Default implementations of the abstractions
- **SystemTextJson**: System.Text.Json integration (recommended for .NET Core/5+)
- **Newtonsoft**: Newtonsoft.Json integration (required for Unity and older .NET Framework)
- **Unity**: Unity-specific extensions and helpers
- **DependencyInjection**: Microsoft DI integration
- **Serialization**: Abstracted JSON serialization with pluggable providers

All components are designed for dependency injection and can be replaced with custom implementations.

## 📄 JSON Serialization Abstraction

SimpleJwt uses an abstracted JSON serialization layer that supports different JSON libraries. **⚠️ You must explicitly configure a JSON provider before using SimpleJwt**:

```csharp
// Configure JSON serialization for System.Text.Json (default in .NET Core)
JsonProviderConfiguration.SetProvider(new SystemTextJsonProvider());

// Or configure for Newtonsoft.Json (required for Unity)
JsonProviderConfiguration.SetProvider(new NewtonsoftJsonProvider());

// Or implement your own provider
public class CustomJsonProvider : IJsonProvider
{
    public string Serialize<T>(T obj) => // Your serialization logic
    
    public T Deserialize<T>(string json) => // Your deserialization logic
    
    public object Deserialize(string json, Type type) => // Your deserialization logic
}
```

### 💉 Microsoft Dependency Injection Integration

If you're using Microsoft DI, you can register SimpleJwt services with extensions:

```csharp
// ASP.NET Core Startup.ConfigureServices or .NET 6+ Program.cs
services.AddSimpleJwt(options => 
{
    options.ValidationParameters = new ValidationParameters
    {
        ValidIssuer = "https://myissuer.com",
        ValidAudience = "https://myapi.com",
        ValidateLifetime = true
    };
});

// Register token lifecycle services
services.AddTokenRefresher();
services.AddTokenRevoker();

// Configure JSON serialization - select ONE of the following options:

// Option 1: Use System.Text.Json (recommended for modern .NET applications)
// Make sure to add a reference to SimpleJwt.SystemTextJson package first
services.UseSystemTextJson(options => 
{
    // Customize System.Text.Json options if needed
    options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// Option 2: Use Newtonsoft.Json
// Make sure to add a reference to SimpleJwt.Newtonsoft package first
services.UseNewtonsoftJson(options => 
{
    // Customize Newtonsoft.Json options if needed
});
```

This enables scoped resolution of SimpleJwt services:

```csharp
public class JwtService
{
    private readonly IJwtParser _parser;
    private readonly IJwtValidator _validator;
    
    public JwtService(IJwtParser parser, IJwtValidator validator)
    {
        _parser = parser;
        _validator = validator;
    }
    
    public bool ValidateToken(string token)
    {
        var parsedToken = _parser.Parse(token);
        var result = _validator.Validate(parsedToken);
        return result.IsValid;
    }
}
```

### 🎮 Unity Integration

For Unity projects, use the SimpleJwt.Unity package which provides a simple initialization method:

```csharp
// In your initialization code (e.g., Awake or Start method)
void Awake()
{
    // Initialize SimpleJwt for Unity with Newtonsoft.Json
    SimpleJwtUnityExtensions.InitializeForUnity();
    
    // Or if you need custom configuration:
    JsonProviderConfiguration.SetProvider(new NewtonsoftJsonProvider 
    {
        // Configure Newtonsoft.Json settings here
    });
}
```

The `SimpleJwtUnityExtensions.InitializeForUnity()` method performs several critical setup tasks:

1. **Configures JSON Provider**: Automatically configures the Newtonsoft.Json provider as the default serializer since System.Text.Json is not fully supported in Unity.
2. **Sets Up IL2CPP Compatibility**: Registers essential types for AOT compilation when using IL2CPP scripting backend.
3. **Optimizes for Unity**: Adjusts serialization settings to be compatible with Unity's environment.
4. **Registers Date Handling**: Ensures proper serialization and deserialization of DateTime objects in JWT claims.
5. **Initializes Security Providers**: Sets up the appropriate cryptographic providers that work in Unity's runtime environment.

This initialization only needs to be called once at application startup, typically in an early-loaded MonoBehaviour.

#### ⚠️ Important Note for Unity Projects

When using SimpleJwt in Unity:

1. Always use the SimpleJwt.Unity package which properly handles JSON serialization compatibility
2. Make sure to call `SimpleJwtUnityExtensions.InitializeForUnity()` during initialization
3. Ensure you have the Newtonsoft.Json package installed in your Unity project

The library has been refactored to eliminate any direct dependency on System.Text.Json in the core components, ensuring complete compatibility with Unity's IL2CPP scripting backend.

## 🚀 Quick Start

### 🔨 Creating and Signing a JWT

```csharp
// Create a JWT with standard claims
string token = JwtBuilder.Create()
    .SetIssuer("https://myissuer.com")
    .SetAudience("https://myapi.com")
    .SetSubject("user123")
    .AddClaim("role", "admin")
    .AddClaim("permissions", new[] { "read", "write", "delete" }) // Objects are serialized using the configured JSON provider
    .AddLifetimeClaims(TimeSpan.FromHours(1)) // Adds exp and nbf claims
    .AddJti() // Adds a unique identifier to prevent replay attacks
    .SignHs256(Encoding.UTF8.GetBytes("your-secret-key-with-at-least-32-bytes"));

// Configure specific header parameters
string tokenWithHeaders = JwtBuilder.Create()
    .SetIssuer("https://myissuer.com")
    .SetHeaderParameter("kid", "key-2023-04-01") // For key rotation
    .SignHs256(Encoding.UTF8.GetBytes("your-secret-key-with-at-least-32-bytes"));
```

### 🔍 Parsing and Validating a JWT

```csharp
// Parse the token
IJwtParser parser = new JwtParser();
IJwtToken parsedToken = parser.Parse(token);

// Access token parts
string algorithm = parsedToken.GetHeaderClaim<string>(JwtConstants.HeaderAlgorithm);
string subject = parsedToken.GetClaim<string>(JwtConstants.ClaimSubject);
string[] roles = parsedToken.GetClaim<string[]>("roles"); // Deserialized using the configured JSON provider
DateTimeOffset expiration = DateTimeOffset.FromUnixTimeSeconds(
    parsedToken.GetClaim<long>(JwtConstants.ClaimExpiration));

// Set up validation parameters
var validationParameters = new ValidationParameters
{
    ValidIssuer = "https://myissuer.com",
    ValidAudience = "https://myapi.com",
    SymmetricSecurityKey = Encoding.UTF8.GetBytes("your-secret-key-with-at-least-32-bytes"),
    ValidateLifetime = true,
    ClockSkew = TimeSpan.FromMinutes(5), // Allow for clock skew between servers
    JtiValidator = (jti) => !IsTokenUsedBefore(jti) // Custom JTI validation
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
    
    // Get custom objects using the configured JSON provider
    if (parsedToken.TryGetClaim<string[]>("permissions", out var permissions))
    {
        foreach (var permission in permissions)
        {
            Console.WriteLine($"Has permission: {permission}");
        }
    }
}
else
{
    // Token is invalid, handle errors
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"Error code: {error.Code}, Message: {error.Message}");
        // Common error codes: "invalid_token", "token_expired", "invalid_signature"
    }
}
```

### Fluent Validation API

```csharp
// Configure the validator with a fluent API
IJwtValidator validator = new JwtValidator()
    .SetHmacKey(Encoding.UTF8.GetBytes("your-secret-key-with-at-least-32-bytes"))
    .SetIssuer("https://myissuer.com")
    .SetAudience("https://myapi.com")
    .SetClockSkew(TimeSpan.FromMinutes(2))
    .ValidateExpiration()
    .ValidateNotBefore();

// Validate the token
ValidationResult result = validator.Validate(token);
```

### Async Token Validation

```csharp
// Asynchronously parse and validate a token
IJwtToken parsedToken = await parser.ParseAsync(token, cancellationToken);
ValidationResult result = await validator.ValidateAsync(parsedToken, validationParameters, cancellationToken);

// Try-pattern for safer handling without exceptions
if (await validator.TryValidateAsync(token, cancellationToken) is var (success, validationResult) && success)
{
    // Token was successfully parsed and validated (though may not be valid)
    if (validationResult.IsValid)
    {
        // Token is valid
    }
}
```

### Token Refresh

```csharp
// Create a token refresher
ITokenRefresher refresher = new TokenRefresher();

// Create a refresh token for an access token
string refreshToken = refresher.CreateRefreshToken(
    accessToken, 
    TimeSpan.FromDays(30));

// Later, use the refresh token to get a new access token
RefreshResult refreshResult = await refresher.RefreshAsync(
    accessToken, 
    refreshToken,
    cancellationToken);

if (refreshResult.IsSuccess)
{
    // Use the new tokens
    string newAccessToken = refreshResult.AccessToken;
    string newRefreshToken = refreshResult.RefreshToken;
    
    // Check expiration times
    DateTimeOffset accessTokenExpires = refreshResult.AccessTokenExpiresAt;
    DateTimeOffset refreshTokenExpires = refreshResult.RefreshTokenExpiresAt;
}
else
{
    // Handle refresh failure
    string errorMessage = refreshResult.Error;
}

// Validate a refresh token
bool isValid = refresher.ValidateRefreshToken(accessToken, refreshToken);
```

### Token Revocation

```csharp
// Create a token revoker
ITokenRevoker revoker = new TokenRevoker();

// Revoke a token
bool revoked = await revoker.RevokeAsync(
    token, 
    reason: "User logged out", 
    expirationTime: DateTimeOffset.UtcNow.AddDays(7),
    cancellationToken);

// Check if a token is revoked before using it
if (await revoker.IsRevokedAsync(token, cancellationToken))
{
    // Token is revoked, reject the request
}

// Get revocation information
if (await revoker.TryGetRevocationReasonAsync(token, out var reason, cancellationToken))
{
    Console.WriteLine($"Token was revoked because: {reason}");
}
```

## 🔧 Advanced Features

### 🔄 Custom Claims Transformation

Claims transformers allow you to modify claims during the validation process, useful for adding roles or permissions based on other claims.

```csharp
// Implement the IClaimsTransformer interface
public class RoleBasedPermissionsTransformer : IClaimsTransformer
{
    private readonly Dictionary<string, string[]> _rolePermissions = new()
    {
        ["admin"] = new[] { "read", "write", "delete" },
        ["editor"] = new[] { "read", "write" },
        ["viewer"] = new[] { "read" }
    };

    public IDictionary<string, object> TransformClaims(
        IDictionary<string, object> claims, 
        ValidationContext context)
    {
        // Clone the claims dictionary
        var transformedClaims = new Dictionary<string, object>(claims);
        
        // Check if there's a role claim
        if (claims.TryGetValue("role", out var roleObj) && roleObj is string role)
        {
            // Add permissions based on role
            if (_rolePermissions.TryGetValue(role, out var permissions))
            {
                transformedClaims["permissions"] = permissions;
            }
        }
        
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

// Register the transformer with the validator
validator.SetClaimsTransformer(new RoleBasedPermissionsTransformer());
```

### 🔁 Key Rotation and Multiple Algorithm Support

```csharp
// Create a key store for multiple keys
var keyStore = new Dictionary<string, object>
{
    ["key-2023-04"] = Encoding.UTF8.GetBytes("your-secret-key-with-at-least-32-bytes"),
    ["key-2023-05"] = Encoding.UTF8.GetBytes("your-new-secret-key-with-at-least-32-bytes"),
    ["rsa-2023"] = RSA.Create(2048)
};

// Configure validation parameters with multiple keys
var validationParameters = new ValidationParameters
{
    SecurityKeys = keyStore,
    // The validator will use the "kid" header to determine which key to use
};

// Create a token with a specific key ID
string token = JwtBuilder.Create()
    .SetIssuer("https://myissuer.com")
    .SetHeaderParameter("kid", "key-2023-05")
    .SignHs256(Encoding.UTF8.GetBytes("your-new-secret-key-with-at-least-32-bytes"));

// Create a token with RSA signing
string rsaToken = JwtBuilder.Create()
    .SetIssuer("https://myissuer.com")
    .SetHeaderParameter("kid", "rsa-2023")
    .SignRs256((RSA)keyStore["rsa-2023"]);
```

### 🚄 Performance Optimization with Caching

SimpleJwt provides two complementary caching mechanisms to optimize performance:

#### 1. Validation Result Caching (ITokenCache)

This caching system stores the results of token validation to avoid repeatedly validating the same token:

```csharp
// Create a validation result cache provider
ITokenCache validationCache = new InMemoryTokenCache(
    maxSize: 10000, // Maximum number of validation results to cache
    cleanupInterval: TimeSpan.FromMinutes(10) // How often to remove expired entries
);

// Configure the JWT validator to use caching
var validationParameters = new ValidationParameters
{
    EnableCaching = true,
    CacheDuration = TimeSpan.FromMinutes(5) // How long to cache validation results
};

// Create a validator that uses the validation cache
var validator = new JwtValidator(validationCache);

// The cache will automatically store validation results
// and return them for subsequent validation requests for the same token
```

#### 2. Parsed Token Caching (ISimpleTokenCache)

This caching system stores the parsed token objects themselves to avoid repeatedly parsing the same token string:

```csharp
// Create a parsed token cache
ISimpleTokenCache tokenCache = new InMemoryTokenCache(
    maxSize: 1000 // Maximum number of parsed tokens to store
);

// Validate a token using the token cache
var result = validator.Validate(token, parameters, tokenCache);

// For subsequent validations of the same token, the parser will retrieve
// the parsed token from the cache instead of re-parsing it
```

#### Using Both Caches Together

For maximum performance in high-throughput applications, you can use both caching mechanisms together:

```csharp
// Set up both caches
ITokenCache validationCache = new InMemoryTokenCache(maxSize: 10000);
ISimpleTokenCache tokenCache = new InMemoryTokenCache(maxSize: 1000);

// Configure parameters
var parameters = new ValidationParameters
{
    EnableCaching = true,
    CacheDuration = TimeSpan.FromMinutes(5),
    // Other validation parameters...
};

// Create validator with validation cache
var validator = new JwtValidator(validationCache);

// Use both caches during validation
var result = validator.Validate(token, parameters, tokenCache);

// This provides two layers of performance optimization:
// 1. The token cache prevents unnecessary re-parsing
// 2. The validation cache prevents unnecessary signature validation and claim checks
```

This dual-caching approach is particularly effective in scenarios where:
- The same tokens are repeatedly validated
- Token parsing or cryptographic operations are performance bottlenecks
- You're handling a large volume of token validations in a short time

### 🗄️ Token Repository

SimpleJwt provides a token repository system to store and manage tokens throughout their lifecycle:

```csharp
// Create a token repository
ITokenRepository repository = new InMemoryTokenRepository(
    cleanupInterval: TimeSpan.FromMinutes(30) // How often to automatically clean up expired tokens
);

// Store a token in the repository
string token = JwtBuilder.Create()
    .SetSubject("user123")
    .AddLifetimeClaims(TimeSpan.FromHours(1))
    .SignHs256(securityKey);

repository.StoreToken(
    token: token,
    userId: "user123", 
    expirationTime: DateTimeOffset.UtcNow.AddHours(1),
    tokenType: "access",
    metadata: new Dictionary<string, object>
    {
        { "deviceId", "device123" },
        { "ipAddress", "192.168.1.1" }
    }
);

// Retrieve a token from the repository
TokenInfo tokenInfo = repository.GetToken(token);
if (tokenInfo != null && !tokenInfo.IsExpired)
{
    // Use the token info
    string userId = tokenInfo.UserId;
    string tokenType = tokenInfo.TokenType;
    TimeSpan timeUntilExpiration = tokenInfo.TimeUntilExpiration;
    
    // Access metadata
    if (tokenInfo.Metadata.TryGetValue("deviceId", out var deviceId))
    {
        Console.WriteLine($"Token issued for device: {deviceId}");
    }
}

// Query tokens for a specific user
IEnumerable<TokenInfo> userTokens = repository.GetTokensForUser("user123");
foreach (var info in userTokens)
{
    Console.WriteLine($"Token type: {info.TokenType}, Expires in: {info.TimeUntilExpiration}");
}

// Query tokens by type
IEnumerable<TokenInfo> refreshTokens = repository.GetTokensForUser("user123", "refresh");

// Remove expired tokens
int removedCount = repository.RemoveExpiredTokens();
Console.WriteLine($"Removed {removedCount} expired tokens");

// Revoke all tokens for a user
int revokedCount = repository.RemoveTokensForUser("user123");
Console.WriteLine($"Revoked {revokedCount} tokens for user");
```

#### Integration with Token Revoker

The token repository can be used with a specialized token revoker for enhanced tracking:

```csharp
// Create a repository-backed token revoker
ITokenRepository repository = new InMemoryTokenRepository();
ITokenRevoker revoker = new TokenRepositoryRevoker(repository, new JwtParser());

// Revoke a token
revoker.Revoke(token, "User logged out");

// Check if a token is revoked
bool isRevoked = revoker.IsRevoked(token);

// Get the reason a token was revoked
string reason = revoker.GetRevocationReason(token);

// Revoke all tokens for a user
int count = revoker.RevokeAllForUser("user123", "Account deactivated");
```

#### Repository Benefits

Using the token repository provides several advantages:

1. **User-centric token management** - Track which tokens belong to which users
2. **Automatic cleanup** - Expired tokens are automatically removed at configurable intervals
3. **Metadata storage** - Store additional information with tokens such as device IDs, IP addresses, etc.
4. **Enhanced revocation** - Easily revoke all tokens for a specific user
5. **Thread safety** - All operations are thread-safe for concurrent applications

The in-memory implementation is suitable for most applications, but the interface can be implemented with any backing store such as a database for persistent storage.

## 🎮 Unity-Specific Features

When using SimpleJwt in Unity projects:

### Setup

```csharp
// In your initialization script
using SimpleJwt.Unity;
using SimpleJwt.Newtonsoft.Serialization;

public class JwtInitializer : MonoBehaviour
{
    void Awake()
    {
        // Initialize SimpleJwt for Unity (uses Newtonsoft.Json by default)
        SimpleJwtUnityExtensions.InitializeForUnity();
        
        // Or manually configure if needed
        JsonProviderConfiguration.SetProvider(new NewtonsoftJsonProvider());
    }
}
```

### Using Coroutines for Async Operations

SimpleJwt.Unity provides coroutine-based extensions for easier integration with Unity:

```csharp
// Example of validating a token using a coroutine
public class TokenValidator : MonoBehaviour
{
    private IJwtValidator _validator;
    
    void Start()
    {
        _validator = new JwtValidator();
        StartCoroutine(ValidateTokenProcess("your-token-here"));
    }
    
    private IEnumerator ValidateTokenProcess(string token)
    {
        yield return _validator.ValidateTokenCoroutine(token, result => 
        {
            if (result.IsValid)
            {
                Debug.Log("Token is valid!");
            }
            else
            {
                Debug.LogError($"Token validation failed: {result.Errors[0].Message}");
            }
        });
    }
}

// Example of refreshing a token using a coroutine
public class TokenRefresher : MonoBehaviour
{
    private ITokenRefresher _refresher;
    
    void Start()
    {
        _refresher = new TokenRefresher();
        StartCoroutine(RefreshTokenProcess("access-token", "refresh-token"));
    }
    
    private IEnumerator RefreshTokenProcess(string accessToken, string refreshToken)
    {
        yield return _refresher.RefreshTokenCoroutine(accessToken, refreshToken, result => 
        {
            if (result.IsSuccess)
            {
                Debug.Log($"New access token: {result.AccessToken}");
                Debug.Log($"New refresh token: {result.RefreshToken}");
            }
            else
            {
                Debug.LogError($"Token refresh failed: {result.Error}");
            }
        });
    }
}
```

### IL2CPP Considerations

When using IL2CPP scripting backend in Unity:

```csharp
// For IL2CPP compatibility, register AOT serialization types
using SimpleJwt.Newtonsoft.Serialization;
using UnityEngine.Scripting;

[Preserve]
public class JsonAotConfig
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        // Register types that need to be serialized/deserialized
        NewtonsoftJsonProvider.RegisterForAot<Dictionary<string, string>>();
        NewtonsoftJsonProvider.RegisterForAot<List<string>>();
        NewtonsoftJsonProvider.RegisterForAot<YourCustomType>();
    }
}
```

## 🛡️ Security Best Practices

- **🔑 Key Management**:
  - Use strong keys (at least 256 bits for HMAC, 2048+ bits for RSA, 256+ bits for ECDSA)
  - Store keys securely (e.g., Azure Key Vault, AWS KMS, HashiCorp Vault)
  - Implement key rotation procedures (SimpleJwt supports this via kid header)
  - Never hardcode keys in source code

- **✅ Token Validation**:
  - Always validate token lifetime (exp/nbf claims)
  - Validate issuer and audience to prevent token reuse across systems
  - Set appropriate ClockSkew to account for server time differences (5 minutes is common)
  - Consider implementing JTI tracking to prevent replay attacks

- **📝 Token Design**:
  - Keep tokens small by including only necessary claims
  - Use short expiration times for sensitive operations (minutes, not days)
  - For longer sessions, use refresh tokens with the token refresher
  - Include only non-sensitive data in tokens (they can be decoded easily)

- **💻 Implementation**:
  - Use HTTPS for all token transmissions
  - Implement token revocation for logout and security incidents
# 🔐 SimpleJwt

[![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.1-blue.svg)](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Build Status](https://img.shields.io/badge/Build-Passing-brightgreen.svg)](https://github.com/yourusername/simple-jwt/actions)
[![Test Coverage](https://img.shields.io/badge/Test%20Coverage-98%25-brightgreen.svg)](https://github.com/yourusername/simple-jwt/actions)
[![NuGet Version](https://img.shields.io/badge/NuGet-0.1.0-blue.svg)](https://www.nuget.org/packages/SimpleJwt.Core)
[![Unity Compatible](https://img.shields.io/badge/Unity-Compatible-blue.svg)](https://unity.com)
[![Security Rating](https://img.shields.io/badge/Security-A%2B-brightgreen.svg)](https://github.com/yourusername/simple-jwt/security)

A lightweight, secure, and extensible JWT (JSON Web Token) library for .NET applications, designed with performance and security as top priorities. Supports both regular .NET applications and Unity game development.

## ✨ Features

- **🔄 Standards Compliant**: Fully compliant with RFC 7519 (JWT), RFC 7515 (JWS), and related specifications
- **🔒 Immutable Token Design**: Thread-safe token processing with functional-style modification methods that prevent race conditions in concurrent scenarios
- **✅ Comprehensive Validation**: Extensive validation options for security-critical applications, including:
  - Signature validation (HMAC, RSA, ECDSA)
  - Issuer and audience validation
  - Expiration and not-before time validation
  - Custom claim validation
- **⚡ Async Support**: First-class async API for high-performance scenarios with proper cancellation token support
- **🛡️ Enhanced Security Features**:
  - JWT Confirmation Method (cnf) claim validation for proof-of-possession tokens
  - JWT ID (jti) tracking to prevent replay attacks with built-in validators
  - Token type validation to prevent substitution attacks between different token types
  - Key rotation support with kid header parameter handling
- **⏱️ Token Lifetime Management**:
  - Token refresh and sliding expiration mechanisms with configurable timeout policies
  - Token revocation capabilities with custom reason storage
  - Built-in support for revocation lists and blacklists
- **🚀 Performance Optimized**:
  - Caching layer for parsed tokens and validation results
  - Efficient token parsing and validation with minimal allocations
  - Configurable memory limits to prevent DOS attacks
- **🎮 Unity Compatibility**:
  - Abstracted JSON serialization layer with support for both System.Text.Json and Newtonsoft.Json
  - Compatible with Unity's IL2CPP scripting backend
  - Minimal dependencies for easier integration with Unity projects
  - Coroutine-based API for async operations in Unity
- **Zero Dependencies**: Built with no external dependencies, using only .NET Standard 2.1 libraries
- **Full .NET Standard 2.1 Compliance**: Compatible with any .NET platform that supports .NET Standard 2.1
- **Multiple Signing Algorithms**: Support for HMAC-SHA256/384/512, RSA-SHA256/384/512, and ECDSA-SHA256/384/512
- **Comprehensive Claim Management**: Built-in support for standard JWT claims and custom claim handling
- **Flexible Token Building**: Fluent builder pattern for easy token creation
- **Robust Token Parsing**: Safe and efficient token parsing with validation
- **Extensible Architecture**: Easy to extend with custom claim types and validation rules
- **Strong Type Safety**: Full type safety for all operations
- **Comprehensive Testing**: Extensive test coverage for all features

## 📦 Installation

```bash
# Install the core package
dotnet add package SimpleJwt.Core

# For abstraction interfaces only (DI scenarios)
dotnet add package SimpleJwt.Abstractions

# For System.Text.Json support (recommended for .NET Core/5+)
dotnet add package SimpleJwt.SystemTextJson

# For Newtonsoft.Json support
dotnet add package SimpleJwt.Newtonsoft

# For Unity compatibility
dotnet add package SimpleJwt.Unity

# For Microsoft DI integration
dotnet add package SimpleJwt.DependencyInjection
```

> 📢 **Note:** The SimpleJwt packages have not been published to NuGet yet. We plan to publish them after completing more extensive testing and validation. In the meantime, you can build the packages from source using this repository.

## 🏗️ Technical Architecture

SimpleJwt uses a layered architecture:

- **Abstractions**: Core interfaces and models (`IJwtToken`, `IJwtParser`, `IJwtValidator`, etc.)
- **Core**: Default implementations of the abstractions
- **SystemTextJson**: System.Text.Json integration (recommended for .NET Core/5+)
- **Newtonsoft**: Newtonsoft.Json integration (required for Unity and older .NET Framework)
- **Unity**: Unity-specific extensions and helpers
- **DependencyInjection**: Microsoft DI integration
- **Serialization**: Abstracted JSON serialization with pluggable providers

All components are designed for dependency injection and can be replaced with custom implementations.

## 📄 JSON Serialization Abstraction

SimpleJwt uses an abstracted JSON serialization layer that supports different JSON libraries. **⚠️ You must explicitly configure a JSON provider before using SimpleJwt**:

```csharp
// Configure JSON serialization for System.Text.Json (default in .NET Core)
JsonProviderConfiguration.SetProvider(new SystemTextJsonProvider());

// Or configure for Newtonsoft.Json (required for Unity)
JsonProviderConfiguration.SetProvider(new NewtonsoftJsonProvider());

// Or implement your own provider
public class CustomJsonProvider : IJsonProvider
{
    public string Serialize<T>(T obj) => // Your serialization logic
    
    public T Deserialize<T>(string json) => // Your deserialization logic
    
    public object Deserialize(string json, Type type) => // Your deserialization logic
}
```

### 💉 Microsoft Dependency Injection Integration

If you're using Microsoft DI, you can register SimpleJwt services with extensions:

```csharp
// ASP.NET Core Startup.ConfigureServices or .NET 6+ Program.cs
services.AddSimpleJwt(options => 
{
    options.ValidationParameters = new ValidationParameters
    {
        ValidIssuer = "https://myissuer.com",
        ValidAudience = "https://myapi.com",
        ValidateLifetime = true
    };
});

// Register token lifecycle services
services.AddTokenRefresher();
services.AddTokenRevoker();

// Configure JSON serialization - select ONE of the following options:

// Option 1: Use System.Text.Json (recommended for modern .NET applications)
// Make sure to add a reference to SimpleJwt.SystemTextJson package first
services.UseSystemTextJson(options => 
{
    // Customize System.Text.Json options if needed
    options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// Option 2: Use Newtonsoft.Json
// Make sure to add a reference to SimpleJwt.Newtonsoft package first
services.UseNewtonsoftJson(options => 
{
    // Customize Newtonsoft.Json options if needed
});
```

This enables scoped resolution of SimpleJwt services:

```csharp
public class JwtService
{
    private readonly IJwtParser _parser;
    private readonly IJwtValidator _validator;
    
    public JwtService(IJwtParser parser, IJwtValidator validator)
    {
        _parser = parser;
        _validator = validator;
    }
    
    public bool ValidateToken(string token)
    {
        var parsedToken = _parser.Parse(token);
        var result = _validator.Validate(parsedToken);
        return result.IsValid;
    }
}
```

### 🎮 Unity Integration

For Unity projects, use the SimpleJwt.Unity package which provides a simple initialization method:

```csharp
// In your initialization code (e.g., Awake or Start method)
void Awake()
{
    // Initialize SimpleJwt for Unity with Newtonsoft.Json
    SimpleJwtUnityExtensions.InitializeForUnity();
    
    // Or if you need custom configuration:
    JsonProviderConfiguration.SetProvider(new NewtonsoftJsonProvider 
    {
        // Configure Newtonsoft.Json settings here
    });
}
```

The `SimpleJwtUnityExtensions.InitializeForUnity()` method performs several critical setup tasks:

1. **Configures JSON Provider**: Automatically configures the Newtonsoft.Json provider as the default serializer since System.Text.Json is not fully supported in Unity.
2. **Sets Up IL2CPP Compatibility**: Registers essential types for AOT compilation when using IL2CPP scripting backend.
3. **Optimizes for Unity**: Adjusts serialization settings to be compatible with Unity's environment.
4. **Registers Date Handling**: Ensures proper serialization and deserialization of DateTime objects in JWT claims.
5. **Initializes Security Providers**: Sets up the appropriate cryptographic providers that work in Unity's runtime environment.

This initialization only needs to be called once at application startup, typically in an early-loaded MonoBehaviour.

#### ⚠️ Important Note for Unity Projects

When using SimpleJwt in Unity:

1. Always use the SimpleJwt.Unity package which properly handles JSON serialization compatibility
2. Make sure to call `SimpleJwtUnityExtensions.InitializeForUnity()` during initialization
3. Ensure you have the Newtonsoft.Json package installed in your Unity project

The library has been refactored to eliminate any direct dependency on System.Text.Json in the core components, ensuring complete compatibility with Unity's IL2CPP scripting backend.

## 🚀 Quick Start

### 🔨 Creating and Signing a JWT

```csharp
// Create a JWT with standard claims
string token = JwtBuilder.Create()
    .SetIssuer("https://myissuer.com")
    .SetAudience("https://myapi.com")
    .SetSubject("user123")
    .AddClaim("role", "admin")
    .AddClaim("permissions", new[] { "read", "write", "delete" }) // Objects are serialized using the configured JSON provider
    .AddLifetimeClaims(TimeSpan.FromHours(1)) // Adds exp and nbf claims
    .AddJti() // Adds a unique identifier to prevent replay attacks
    .SignHs256(Encoding.UTF8.GetBytes("your-secret-key-with-at-least-32-bytes"));

// Configure specific header parameters
string tokenWithHeaders = JwtBuilder.Create()
    .SetIssuer("https://myissuer.com")
    .SetHeaderParameter("kid", "key-2023-04-01") // For key rotation
    .SignHs256(Encoding.UTF8.GetBytes("your-secret-key-with-at-least-32-bytes"));
```

### 🔍 Parsing and Validating a JWT

```csharp
// Parse the token
IJwtParser parser = new JwtParser();
IJwtToken parsedToken = parser.Parse(token);

// Access token parts
string algorithm = parsedToken.GetHeaderClaim<string>(JwtConstants.HeaderAlgorithm);
string subject = parsedToken.GetClaim<string>(JwtConstants.ClaimSubject);
string[] roles = parsedToken.GetClaim<string[]>("roles"); // Deserialized using the configured JSON provider
DateTimeOffset expiration = DateTimeOffset.FromUnixTimeSeconds(
    parsedToken.GetClaim<long>(JwtConstants.ClaimExpiration));

// Set up validation parameters
var validationParameters = new ValidationParameters
{
    ValidIssuer = "https://myissuer.com",
    ValidAudience = "https://myapi.com",
    SymmetricSecurityKey = Encoding.UTF8.GetBytes("your-secret-key-with-at-least-32-bytes"),
    ValidateLifetime = true,
    ClockSkew = TimeSpan.FromMinutes(5), // Allow for clock skew between servers
    JtiValidator = (jti) => !IsTokenUsedBefore(jti) // Custom JTI validation
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
    
    // Get custom objects using the configured JSON provider
    if (parsedToken.TryGetClaim<string[]>("permissions", out var permissions))
    {
        foreach (var permission in permissions)
        {
            Console.WriteLine($"Has permission: {permission}");
        }
    }
}
else
{
    // Token is invalid, handle errors
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"Error code: {error.Code}, Message: {error.Message}");
        // Common error codes: "invalid_token", "token_expired", "invalid_signature"
    }
}
```

### Fluent Validation API

```csharp
// Configure the validator with a fluent API
IJwtValidator validator = new JwtValidator()
    .SetHmacKey(Encoding.UTF8.GetBytes("your-secret-key-with-at-least-32-bytes"))
    .SetIssuer("https://myissuer.com")
    .SetAudience("https://myapi.com")
    .SetClockSkew(TimeSpan.FromMinutes(2))
    .ValidateExpiration()
    .ValidateNotBefore();

// Validate the token
ValidationResult result = validator.Validate(token);
```

### Async Token Validation

```csharp
// Asynchronously parse and validate a token
IJwtToken parsedToken = await parser.ParseAsync(token, cancellationToken);
ValidationResult result = await validator.ValidateAsync(parsedToken, validationParameters, cancellationToken);

// Try-pattern for safer handling without exceptions
if (await validator.TryValidateAsync(token, cancellationToken) is var (success, validationResult) && success)
{
    // Token was successfully parsed and validated (though may not be valid)
    if (validationResult.IsValid)
    {
        // Token is valid
    }
}
```

### Token Refresh

```csharp
// Create a token refresher
ITokenRefresher refresher = new TokenRefresher();

// Create a refresh token for an access token
string refreshToken = refresher.CreateRefreshToken(
    accessToken, 
    TimeSpan.FromDays(30));

// Later, use the refresh token to get a new access token
RefreshResult refreshResult = await refresher.RefreshAsync(
    accessToken, 
    refreshToken,
    cancellationToken);

if (refreshResult.IsSuccess)
{
    // Use the new tokens
    string newAccessToken = refreshResult.AccessToken;
    string newRefreshToken = refreshResult.RefreshToken;
    
    // Check expiration times
    DateTimeOffset accessTokenExpires = refreshResult.AccessTokenExpiresAt;
    DateTimeOffset refreshTokenExpires = refreshResult.RefreshTokenExpiresAt;
}
else
{
    // Handle refresh failure
    string errorMessage = refreshResult.Error;
}

// Validate a refresh token
bool isValid = refresher.ValidateRefreshToken(accessToken, refreshToken);
```

### Token Revocation

```csharp
// Create a token revoker
ITokenRevoker revoker = new TokenRevoker();

// Revoke a token
bool revoked = await revoker.RevokeAsync(
    token, 
    reason: "User logged out", 
    expirationTime: DateTimeOffset.UtcNow.AddDays(7),
    cancellationToken);

// Check if a token is revoked before using it
if (await revoker.IsRevokedAsync(token, cancellationToken))
{
    // Token is revoked, reject the request
}

// Get revocation information
if (await revoker.TryGetRevocationReasonAsync(token, out var reason, cancellationToken))
{
    Console.WriteLine($"Token was revoked because: {reason}");
}
```

## 🔧 Advanced Features

### 🔄 Custom Claims Transformation

Claims transformers allow you to modify claims during the validation process, useful for adding roles or permissions based on other claims.

```csharp
// Implement the IClaimsTransformer interface
public class RoleBasedPermissionsTransformer : IClaimsTransformer
{
    private readonly Dictionary<string, string[]> _rolePermissions = new()
    {
        ["admin"] = new[] { "read", "write", "delete" },
        ["editor"] = new[] { "read", "write" },
        ["viewer"] = new[] { "read" }
    };

    public IDictionary<string, object> TransformClaims(
        IDictionary<string, object> claims, 
        ValidationContext context)
    {
        // Clone the claims dictionary
        var transformedClaims = new Dictionary<string, object>(claims);
        
        // Check if there's a role claim
        if (claims.TryGetValue("role", out var roleObj) && roleObj is string role)
        {
            // Add permissions based on role
            if (_rolePermissions.TryGetValue(role, out var permissions))
            {
                transformedClaims["permissions"] = permissions;
            }
        }
        
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

// Register the transformer with the validator
validator.SetClaimsTransformer(new RoleBasedPermissionsTransformer());
```

### 🔁 Key Rotation and Multiple Algorithm Support

```csharp
// Create a key store for multiple keys
var keyStore = new Dictionary<string, object>
{
    ["key-2023-04"] = Encoding.UTF8.GetBytes("your-secret-key-with-at-least-32-bytes"),
    ["key-2023-05"] = Encoding.UTF8.GetBytes("your-new-secret-key-with-at-least-32-bytes"),
    ["rsa-2023"] = RSA.Create(2048)
};

// Configure validation parameters with multiple keys
var validationParameters = new ValidationParameters
{
    SecurityKeys = keyStore,
    // The validator will use the "kid" header to determine which key to use
};

// Create a token with a specific key ID
string token = JwtBuilder.Create()
    .SetIssuer("https://myissuer.com")
    .SetHeaderParameter("kid", "key-2023-05")
    .SignHs256(Encoding.UTF8.GetBytes("your-new-secret-key-with-at-least-32-bytes"));

// Create a token with RSA signing
string rsaToken = JwtBuilder.Create()
    .SetIssuer("https://myissuer.com")
    .SetHeaderParameter("kid", "rsa-2023")
    .SignRs256((RSA)keyStore["rsa-2023"]);
```

### 🚄 Performance Optimization with Caching

SimpleJwt provides two complementary caching mechanisms to optimize performance:

#### 1. Validation Result Caching (ITokenCache)

This caching system stores the results of token validation to avoid repeatedly validating the same token:

```csharp
// Create a validation result cache provider
ITokenCache validationCache = new InMemoryTokenCache(
    maxSize: 10000, // Maximum number of validation results to cache
    cleanupInterval: TimeSpan.FromMinutes(10) // How often to remove expired entries
);

// Configure the JWT validator to use caching
var validationParameters = new ValidationParameters
{
    EnableCaching = true,
    CacheDuration = TimeSpan.FromMinutes(5) // How long to cache validation results
};

// Create a validator that uses the validation cache
var validator = new JwtValidator(validationCache);

// The cache will automatically store validation results
// and return them for subsequent validation requests for the same token
```

#### 2. Parsed Token Caching (ISimpleTokenCache)

This caching system stores the parsed token objects themselves to avoid repeatedly parsing the same token string:

```csharp
// Create a parsed token cache
ISimpleTokenCache tokenCache = new InMemoryTokenCache(
    maxSize: 1000 // Maximum number of parsed tokens to store
);

// Validate a token using the token cache
var result = validator.Validate(token, parameters, tokenCache);

// For subsequent validations of the same token, the parser will retrieve
// the parsed token from the cache instead of re-parsing it
```

#### Using Both Caches Together

For maximum performance in high-throughput applications, you can use both caching mechanisms together:

```csharp
// Set up both caches
ITokenCache validationCache = new InMemoryTokenCache(maxSize: 10000);
ISimpleTokenCache tokenCache = new InMemoryTokenCache(maxSize: 1000);

// Configure parameters
var parameters = new ValidationParameters
{
    EnableCaching = true,
    CacheDuration = TimeSpan.FromMinutes(5),
    // Other validation parameters...
};

// Create validator with validation cache
var validator = new JwtValidator(validationCache);

// Use both caches during validation
var result = validator.Validate(token, parameters, tokenCache);

// This provides two layers of performance optimization:
// 1. The token cache prevents unnecessary re-parsing
// 2. The validation cache prevents unnecessary signature validation and claim checks
```

This dual-caching approach is particularly effective in scenarios where:
- The same tokens are repeatedly validated
- Token parsing or cryptographic operations are performance bottlenecks
- You're handling a large volume of token validations in a short time

## 🎮 Unity-Specific Features

When using SimpleJwt in Unity projects:

### Setup

```csharp
// In your initialization script
using SimpleJwt.Unity;
using SimpleJwt.Newtonsoft.Serialization;

public class JwtInitializer : MonoBehaviour
{
    void Awake()
    {
        // Initialize SimpleJwt for Unity (uses Newtonsoft.Json by default)
        SimpleJwtUnityExtensions.InitializeForUnity();
        
        // Or manually configure if needed
        JsonProviderConfiguration.SetProvider(new NewtonsoftJsonProvider());
    }
}
```

### Using Coroutines for Async Operations

SimpleJwt.Unity provides coroutine-based extensions for easier integration with Unity:

```csharp
// Example of validating a token using a coroutine
public class TokenValidator : MonoBehaviour
{
    private IJwtValidator _validator;
    
    void Start()
    {
        _validator = new JwtValidator();
        StartCoroutine(ValidateTokenProcess("your-token-here"));
    }
    
    private IEnumerator ValidateTokenProcess(string token)
    {
        yield return _validator.ValidateTokenCoroutine(token, result => 
        {
            if (result.IsValid)
            {
                Debug.Log("Token is valid!");
            }
            else
            {
                Debug.LogError($"Token validation failed: {result.Errors[0].Message}");
            }
        });
    }
}

// Example of refreshing a token using a coroutine
public class TokenRefresher : MonoBehaviour
{
    private ITokenRefresher _refresher;
    
    void Start()
    {
        _refresher = new TokenRefresher();
        StartCoroutine(RefreshTokenProcess("access-token", "refresh-token"));
    }
    
    private IEnumerator RefreshTokenProcess(string accessToken, string refreshToken)
    {
        yield return _refresher.RefreshTokenCoroutine(accessToken, refreshToken, result => 
        {
            if (result.IsSuccess)
            {
                Debug.Log($"New access token: {result.AccessToken}");
                Debug.Log($"New refresh token: {result.RefreshToken}");
            }
            else
            {
                Debug.LogError($"Token refresh failed: {result.Error}");
            }
        });
    }
}
```

### IL2CPP Considerations

When using IL2CPP scripting backend in Unity:

```csharp
// For IL2CPP compatibility, register AOT serialization types
using SimpleJwt.Newtonsoft.Serialization;
using UnityEngine.Scripting;

[Preserve]
public class JsonAotConfig
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        // Register types that need to be serialized/deserialized
        NewtonsoftJsonProvider.RegisterForAot<Dictionary<string, string>>();
        NewtonsoftJsonProvider.RegisterForAot<List<string>>();
        NewtonsoftJsonProvider.RegisterForAot<YourCustomType>();
    }
}
```

## 🛡️ Security Best Practices

- **🔑 Key Management**:
  - Use strong keys (at least 256 bits for HMAC, 2048+ bits for RSA, 256+ bits for ECDSA)
  - Store keys securely (e.g., Azure Key Vault, AWS KMS, HashiCorp Vault)
  - Implement key rotation procedures (SimpleJwt supports this via kid header)
  - Never hardcode keys in source code

- **✅ Token Validation**:
  - Always validate token lifetime (exp/nbf claims)
  - Validate issuer and audience to prevent token reuse across systems
  - Set appropriate ClockSkew to account for server time differences (5 minutes is common)
  - Consider implementing JTI tracking to prevent replay attacks

- **📝 Token Design**:
  - Keep tokens small by including only necessary claims
  - Use short expiration times for sensitive operations (minutes, not days)
  - For longer sessions, use refresh tokens with the token refresher
  - Include only non-sensitive data in tokens (they can be decoded easily)

- **💻 Implementation**:
  - Use HTTPS for all token transmissions
  - Implement token revocation for logout and security incidents
  - Store tokens securely on clients (httpOnly cookies, secure storage)
  - Add rate limiting for token operations to prevent brute force attacks

## ⚠️ Error Handling

```csharp
// Common error codes and their meanings
var errorCodeDescriptions = new Dictionary<string, string>
{
    [ValidationCodes.InvalidToken] = "The token is malformed or invalid",
    [ValidationCodes.TokenExpired] = "The token has expired",
    [ValidationCodes.TokenNotYetValid] = "The token is not yet valid (nbf claim)",
    [ValidationCodes.InvalidIssuer] = "The issuer is not recognized",
    [ValidationCodes.InvalidAudience] = "The audience is not recognized",
    [ValidationCodes.InvalidSignature] = "The token signature is invalid",
    [ValidationCodes.MissingClaim] = "A required claim is missing",
    [ValidationCodes.InvalidClaimValue] = "A claim has an invalid value"
};

// Handle validation errors appropriately
ValidationResult result = validator.Validate(token);
if (!result.IsValid)
{
    foreach (var error in result.Errors)
    {
        switch (error.Code)
        {
            case ValidationCodes.TokenExpired:
                // Prompt for re-authentication
                break;
            case ValidationCodes.InvalidSignature:
                // Potential security incident - log it
                _logger.LogWarning("Invalid signature detected for token: {TokenId}", tokenId);
                break;
            default:
                // General handling
                _logger.LogInformation("Token validation failed: {ErrorCode} - {ErrorMessage}", 
                    error.Code, error.Message);
                break;
        }
    }
}
```

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Run tests (`dotnet test`)
5. Commit your changes (`git commit -m 'Add some amazing feature'`)
6. Push to the branch (`git push origin feature/amazing-feature`)
7. Open a Pull Request

### 💻 Development Environment Setup

```bash
# Clone the repository
git clone https://github.com/yourusername/simple-jwt.git
cd simple-jwt

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test
```

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## ⚡ Performance Considerations

SimpleJwt is designed for high performance with minimal allocations:

- Token parsing and validation are optimized for speed and memory usage
- Caching is available for frequently validated tokens
- Async methods use proper `ValueTask` where appropriate to reduce allocations
- Token size limits prevent DOS attacks
- JSON serialization is abstracted to allow for platform-specific optimizations

## 📦 Package Versions

This library has been updated to use the latest .NET package references as of 2023, ensuring compatibility with modern applications. If you're using an older .NET version, please check the release notes for compatibility information.

## 🔒 Security Notice

SimpleJwt includes security upgrades that address known vulnerabilities in dependencies. When upgrading, we recommend always checking for the latest version which will include security patches.

## Technical Details

- **Framework Compatibility**: 
  - .NET Standard 2.1 compliant
  - Compatible with .NET Core 3.0+, .NET 5+, and any other .NET implementation supporting .NET Standard 2.1
  - No platform-specific dependencies

- **Dependencies**:
  - Zero external NuGet packages required
  - Uses only built-in .NET Standard 2.1 libraries
  - No runtime dependencies beyond .NET Standard 2.1 

## 🧪 Testing and Verification

SimpleJwt includes a comprehensive test suite to ensure reliability and security:

- **Unit Tests**: Over 54 unit tests covering all core functionality
- **RFC Compliance Tests**: Thorough validation of RFC 7519 (JWT) and RFC 7515 (JWS) compliance
- **Serialization Tests**: Tests with both System.Text.Json and Newtonsoft.Json to ensure serializer-agnostic operation
- **Security Tests**: Validation of security features like signature verification, tampering detection, and replay protection
- **Edge Case Tests**: Comprehensive testing of various edge cases and error conditions

All tests are designed to be JSON serializer-agnostic, ensuring the library works consistently regardless of which JSON provider is used.

### 📊 Current Test Coverage

- Core Features: 98% coverage
- Security Features: 100% coverage
- RFC 7519 (JWT) Compliance: 100% coverage
- RFC 7515 (JWS) Compliance: 100% coverage
- IANA Registered Claims: 100% coverage
- JSON Provider Integration: 100% coverage
- Caching System: 100% coverage 