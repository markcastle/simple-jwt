# 🚀 SimpleJwt.UniCache

This package provides seamless, persistent, and optionally encrypted JWT token caching for the SimpleJwt ecosystem using the [UniCache](https://bitbucket.org/inovus/unicache) library.

## ✨ Features
- 🔌 Plug-and-play persistent token caching for SimpleJwt
- 🔒 AES encryption support for sensitive token data
- 🧩 Compatible with .NET Standard 2.0+
- 🤝 Designed for use with SimpleJwt.Abstractions
- 🗄️ Supports all UniCache backends (memory, persistent, custom)

## 🛠️ Usage

### Basic Usage (with UniCache)
```csharp
using UniCache;
using UniCache.Abstractions;
using SimpleJwt.UniCache;

// Initialize the caching service (memory + persistent path)
ICachingService cachingService = new CachingService(
    new MemoryCache(),
    () => "YourPersistentDataPath/"
);

// Register UniCacheTokenRepository for JWT token storage
services.AddSingleton<ITokenCacheStorage>(
    new UniCacheTokenRepository(cachingService)
);

// Cache a value
cachingService.Set("myKey", "myValue", 60, CacheType.InMemory);

// Retrieve the value
string value = cachingService.Get<string>("myKey");
```

### With Encryption
```csharp
using UniCache;
using UniCache.Abstractions;
using SimpleJwt.UniCache;

// Initialize with encryption
IEncryptionProvider encryptionProvider = new AesEncryptionProvider(
    "yourPassword",   // Use a strong password or key from secure storage
    "yourSalt"        // Use a secure, random salt
);

ICachingService secureCache = new CachingService(
    new MemoryCache(),
    () => "YourPersistentDataPath/",
    encryptionProvider
);

// Register encrypted UniCacheTokenRepository
services.AddSingleton<ITokenCacheStorage>(
    new UniCacheTokenRepository(secureCache)
);

// Cache sensitive data
secureCache.Set("secureKey", "sensitiveData", 60, CacheType.Persistent);
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

See the [UniCache documentation](https://bitbucket.org/inovus/unicache) for more advanced usage, custom backends, and security best practices.

---

_Maintained with ❤️ by the SimpleJwt team. Contributions welcome!_
