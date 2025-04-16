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

## 🧩 Dependency Injection (DI) Integration

SimpleJwt.UniCache provides several flexible ways to register your token cache storage using dependency injection. Choose the method that best fits your application's needs:

| Method | Purpose | Example |
|--------|---------|---------|
| `AddUniCacheTokenRepository()` | Register default UniCacheTokenRepository (uses default UniCache config). | `services.AddUniCacheTokenRepository();` |
| `AddUniCacheTokenRepository(IUniCache, ICacheEncryptionSettings)` | Register with a custom UniCache instance and optional encryption. | `services.AddUniCacheTokenRepository(myCache, myEncryptionSettings);` |
| `AddInMemoryUniCacheTokenRepository()` | Register an in-memory-only cache (ephemeral/testing). | `services.AddInMemoryUniCacheTokenRepository();` |
| `UseUniCache(Func<IUniCache>, Func<ICacheEncryptionSettings>)` | Register with advanced configuration using delegates. | `services.UseUniCache(() => myCache, () => myEncryptionSettings);` |
| `UseCustomCache<T>()` | Register a custom implementation of ITokenCacheStorage. | `services.UseCustomCache<MyCustomCache>();` |

### Examples

#### 1. Default Registration
```csharp
services.AddUniCacheTokenRepository();
```

#### 2. Custom UniCache Instance (with optional encryption)
```csharp
IUniCache myCache = new MemoryCache(); // or your own implementation
ICacheEncryptionSettings myEncryptionSettings = ...; // optional
services.AddUniCacheTokenRepository(myCache, myEncryptionSettings);
```

#### 3. In-Memory Only Cache
```csharp
services.AddInMemoryUniCacheTokenRepository();
```

#### 4. Advanced/Delegate-Based Registration
```csharp
services.UseUniCache(
    () => new MemoryCache(),
    () => new CacheEncryptionSettings(key, salt, 100_000)
);
```

#### 5. Custom Implementation
```csharp
services.UseCustomCache<MyCustomCache>();
```

> ℹ️ **Tip:** For persistent caching, use a persistent UniCache backend and specify the storage path in your implementation.

See the XML comments in `ServiceCollectionExtensions` for further details on each method.

### Example: Persistent UniCache Backend

To use persistent caching, create a persistent UniCache backend by specifying a storage path (such as an app data directory or Unity's `Application.persistentDataPath`).

```csharp
using UniCache;
using UniCache.Abstractions;
using SimpleJwt.UniCache;

// Example: Use a persistent storage path for cache
string persistentPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyAppCache");

ICachingService cachingService = new CachingService(
    new MemoryCache(),
    () => persistentPath
);

// Register with DI or use directly
services.AddUniCacheTokenRepository(cachingService);

// Or, in Unity:
// string persistentPath = Application.persistentDataPath;
// ICachingService cachingService = new CachingService(new MemoryCache(), () => persistentPath);
```

> 🗄️ **Tip:** The persistent path should be a directory where your app has write permissions. On desktop, use `Environment.SpecialFolder.ApplicationData`; in Unity, use `Application.persistentDataPath`.

## 🕹️ Unity Integration

SimpleJwt.UniCache works seamlessly in Unity projects! You do not need a DI container—just instantiate and use the cache directly in your MonoBehaviour or ScriptableObject classes.

### Example: Using UniCache in Unity
```csharp
using UnityEngine;
using UniCache;
using UniCache.Abstractions;
using SimpleJwt.UniCache;

public class JwtTokenManager : MonoBehaviour
{
    private ITokenCacheStorage tokenCache;

    void Awake()
    {
        // Use Unity's persistent data path for file-based cache
        string persistentPath = Application.persistentDataPath;

        // Set up UniCache for Unity (memory + persistent path)
        ICachingService cachingService = new CachingService(
            new MemoryCache(),
            () => persistentPath
        );

        // Optionally, add encryption
        // IEncryptionProvider encryptionProvider = new AesEncryptionProvider("password", "salt");
        // cachingService = new CachingService(new MemoryCache(), () => persistentPath, encryptionProvider);

        // Register with SimpleJwt.UniCache (no DI container needed)
        tokenCache = new UniCacheTokenRepository(cachingService);
    }

    public void StoreToken(string key, IJwtToken token)
    {
        // Use tokenCache.SetTokenAsync(...) as needed
    }
}
```

> ℹ️ **Tip:** Use `Application.persistentDataPath` for cross-platform persistence in Unity. Encryption works the same as in .NET Standard projects.

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
