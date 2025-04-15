# 🗃️ Flexible Caching Architecture for SimpleJwt

SimpleJwt now includes a provider-agnostic caching system for JWT tokens and validation results, supporting both in-memory and persistent storage. The caching system is designed for extensibility, security, and performance.

## Caching Abstractions
- `ICacheProvider<TKey, TValue>`: Generic async cache provider interface.
- `ITokenCacheStorage`: Specialized for JWT tokens.
- `ICacheSerializer`: For serialization/deserialization of cache values.
- `ICacheEncryptionProvider`: For encrypting/decrypting cache values.

## In-Memory Cache Provider
- `SimpleMemoryCacheProvider<TKey, TValue>`: High-performance, thread-safe in-memory cache with FIFO eviction and configurable size limits.
- Fully tested for concurrency, expiry, and edge cases.

## Usage Example
```csharp
var cache = new SimpleMemoryCacheProvider<string, MyToken>(maxSize: 100);
await cache.SetAsync("tokenKey", myToken, TimeSpan.FromMinutes(10));
var token = await cache.GetAsync("tokenKey");
```

## Extending with UniCache (Persistent/Encrypted)
- UniCache integration is supported for persistent and encrypted cache scenarios.
- To use UniCache, install the [UniCache NuGet package](https://www.nuget.org/packages/UniCache) and implement the `ICacheProvider` interface.
- Encryption is supported via `ICacheEncryptionProvider` (AES recommended).

## Security Best Practices
- Always encrypt persisted tokens with a strong encryption provider (AES-256 recommended).
- Use secure key storage and enable key rotation for long-term security.
- Never store sensitive tokens unencrypted on disk.

## Provider Registration (DI)
```csharp
// Register the default in-memory cache:
services.AddSingleton<ICacheProvider<string, IJwtToken>, SimpleMemoryCacheProvider<string, IJwtToken>>();

// (Planned) Register UniCache or custom providers:
// services.AddSingleton<ICacheProvider<string, IJwtToken>, UniCacheProvider<string, IJwtToken>>();
```
