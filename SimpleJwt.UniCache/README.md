# 🚀 SimpleJwt.UniCache

This package provides seamless, persistent, and optionally encrypted JWT token caching for the SimpleJwt ecosystem using the UniCache library.

## ✨ Features
- 🔌 Plug-and-play persistent token caching for SimpleJwt
- 🔒 AES encryption support for sensitive token data
- 🛡️ Key rotation and secure key management
- 🧩 Compatible with .NET Standard 2.0+
- 🤝 Designed for use with SimpleJwt.Abstractions
- 🗄️ Supports file, in-memory, and custom UniCache backends
- 🧪 Full test suite for encryption, persistence, and DI integration

## 🛠️ Usage

1. **Install the NuGet packages:**
   - `SimpleJwt.UniCache` (this package)
   - `UniCache`
   - `UniCache.Encryption`

2. **Register the UniCache token repository in your DI setup:**

```csharp
services.AddUniCacheTokenRepository();
```

3. **Configure encryption and UniCache options as needed:**
   - Use `CacheEncryptionSettings` to define your key, salt, and iteration count.
   - Store keys securely (e.g., Azure Key Vault, AWS KMS, environment variables).

## 💡 Example

```csharp
// Register UniCache-based token storage
services.AddUniCacheTokenRepository();

// Optionally configure with custom cache and encryption
services.AddUniCacheTokenRepository(new MemoryUniCache(), new CacheEncryptionSettings(key, salt, 100_000));

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

## 🧪 Test Coverage
- [x] AES encryption and decryption
- [x] Key rotation
- [x] Token storage and retrieval
- [x] Persistent file/disk cache
- [x] Dependency injection registration
- [x] Cancellation and error handling
- [x] Performance benchmarks for encryption

## 🔐 Security Best Practices
- Always enable encryption for persisted tokens
- Store encryption keys securely (never hardcode in source)
- Rotate keys periodically for long-term security
- Use strong, randomly generated keys and salts

## 📈 Status
- [x] Core implementation complete
- [x] AES encryption and key management
- [x] Full test coverage
- [ ] More backend options (e.g., Redis, distributed cache)
- [ ] Advanced cache eviction policies (LRU/FIFO)
- [ ] Audit logging and monitoring hooks

## 📋 Roadmap & Suggestions
- Add support for distributed cache backends
- Expose cache eviction policy configuration
- Provide admin/ops CLI for cache inspection and management
- Add audit logging for token operations
- Improve documentation with real-world usage scenarios

See the main SimpleJwt documentation for more details.

---

_Maintained with ❤️ by the SimpleJwt team. Contributions welcome!_
