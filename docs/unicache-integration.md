
## 🗃️ Flexible Caching Architecture & UniCache Integration

SimpleJwt features a flexible, provider-agnostic caching system for JWT tokens and validation results. This system supports both high-performance in-memory caching and persistent, encrypted storage using UniCache.

### Key Features
- **Provider-Agnostic**: Use any compatible cache provider (in-memory, UniCache, or custom)
- **Persistent & Encrypted**: UniCache integration allows secure, persistent storage with AES encryption and key rotation
- **Extensible**: Easily swap or extend cache providers via DI
- **Full .NET Standard 2.0+ support**

### How to Use

#### 1. Choose and Register a Cache Provider

**In-Memory (Default):**
```csharp
services.AddInMemoryCache(); // or AddInMemoryUniCacheTokenRepository() for UniCache-based
```

**Persistent/Encrypted (UniCache):**
```csharp
using SimpleJwt.UniCache;

// Example: File-based UniCache with AES encryption
services.UseUniCache(
    () => new FileUniCache("tokens.cache"),
    () => new CacheEncryptionSettings(myKey, mySalt, iterations: 200_000)
);
```
- `myKey`/`mySalt`: Securely generate and store these values. Do NOT hardcode in production.

**Custom Provider:**
```csharp
services.UseCustomCache<MyCustomCacheProvider>();
```

#### 2. Use the Cache in Your Services
```csharp
public class MyService {
    private readonly ITokenCacheStorage _tokenCache;
    public MyService(ITokenCacheStorage tokenCache) {
        _tokenCache = tokenCache;
    }
    public async Task StoreAsync(string key, IJwtToken token) {
        await _tokenCache.SetTokenAsync(key, token);
    }
}
```

#### 3. Security Best Practices
- Always enable encryption for persisted tokens
- Store encryption keys securely and rotate regularly
- Use salt-based key derivation for extra security
- Test your integration with both happy path and error scenarios

### Migration from Previous Versions
See [`docs/migration-caching.md`](./docs/migration-caching.md) for a detailed migration guide, including before/after code samples, troubleshooting, and security tips.

### Advanced Topics
- **Cache Expiry:** Cache entries can be set to expire in alignment with the JWT's lifetime (exp claim)
- **Key Rotation:** Supported via `CacheEncryptionSettings.RotateKey()`
- **Full Test Coverage:** All caching features are covered by unit and integration tests

### Further Reading
- [Caching Architecture Overview](./docs/caching.md)
- [Migration Guide](./docs/migration-caching.md)
- [SimpleJwt.UniCache README](./SimpleJwt.UniCache/README.md)