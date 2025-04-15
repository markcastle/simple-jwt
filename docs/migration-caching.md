# Migration Guide: Flexible Caching Architecture in SimpleJwt

This guide helps you migrate from legacy or in-memory-only caching in SimpleJwt to the new flexible, provider-agnostic caching architecture, including UniCache-based persistent and encrypted storage.

## 1. Overview of New Caching System
- **Provider-agnostic:** Use any compatible cache provider (in-memory, UniCache, custom)
- **Security:** Optional AES encryption and key management for persisted tokens
- **Extensible:** Easily swap or extend cache providers via DI

## 2. Updating Your Dependencies
- **Remove direct references** to legacy or custom cache implementations
- **Add the following NuGet packages as needed:**
  - `SimpleJwt.UniCache` (for persistent/encrypted caching)
  - `UniCache` and `UniCache.Encryption` (for UniCache providers)

## 3. Registering a Cache Provider
### In-Memory (Default)
```csharp
services.AddInMemoryCache(); // or AddInMemoryUniCacheTokenRepository() for UniCache-based
```

### Persistent/Encrypted (UniCache)
```csharp
using SimpleJwt.UniCache;

// Example: File-based UniCache with AES encryption
services.UseUniCache(
    () => new FileUniCache("tokens.cache"),
    () => new CacheEncryptionSettings(myKey, mySalt, iterations: 200_000)
);
```
- **myKey/mySalt:** Securely generate and store these values. Do NOT hardcode in production.

### Custom Provider
```csharp
services.UseCustomCache<MyCustomCacheProvider>();
```

## 4. Migrating Token Storage Logic
- Replace any direct cache usage with the new `ITokenCacheStorage` abstraction
- All cache operations (Get, Set, Remove, Clear) are async and support cancellation tokens

## 5. Security & Best Practices
- Always enable encryption for persisted tokens
- Store keys securely and rotate regularly
- Use salt-based key derivation for extra security
- Test your integration with both happy path and error scenarios

## 6. Example Migration
**Before:**
```csharp
// Old direct in-memory cache usage
var cache = new OldMemoryCache();
cache.Set("token", token);
```

**After:**
```csharp
// New provider-agnostic pattern
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

## 7. Troubleshooting
- If tokens are not retrieved after restart, check encryption keys and cache provider configuration
- For performance issues, benchmark both in-memory and persistent providers under your workload
- Consult the README and docs/caching.md for advanced scenarios

## 8. Further Reading
- [Caching Architecture Overview](./caching.md)
- [SimpleJwt.UniCache README](../SimpleJwt.UniCache/README.md)

---

For any migration questions, please open an issue or consult the main documentation.
