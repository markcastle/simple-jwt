# SimpleJwt.UniCache

This package provides optional UniCache integration for SimpleJwt, enabling persistent and encrypted JWT token caching using the UniCache library.

## Features
- Plug-and-play persistent token caching for SimpleJwt
- AES encryption support for sensitive token data
- Compatible with .NET Standard 2.0+
- Designed for use with SimpleJwt.Abstractions

## Usage

1. Install the NuGet packages:
   - `SimpleJwt.UniCache` (this package)
   - `UniCache`
   - `UniCache.Encryption`

2. Register the UniCache token repository in your DI setup:

```csharp
services.AddUniCacheTokenRepository();
```

3. Configure encryption and UniCache options as needed (see source for details).

## Example

```csharp
// Register UniCache-based token storage
services.AddUniCacheTokenRepository();

// Use ITokenCacheStorage in your application
public class MyService
{
    private readonly ITokenCacheStorage _tokenCache;
    public MyService(ITokenCacheStorage tokenCache)
    {
        _tokenCache = tokenCache;
    }
}
```

## Security Best Practices
- Always enable encryption for persisted tokens
- Store encryption keys securely (do not hardcode)
- Rotate keys periodically for long-term security

## Status
- [ ] Implementation in progress
- [ ] AES encryption and key management
- [ ] Full test coverage

See the main SimpleJwt documentation for more details.
