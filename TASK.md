# SimpleJwt Test Implementation Tasks

## Priority Levels
- 🔴 Critical: Must be implemented first, core functionality and security
- 🟡 High: Important for reliability and compliance
- 🟢 Medium: Important for robustness and edge cases
- ⚪ Low: Nice to have, optimizations and additional features

## 🔴 Critical Priority Tests

### Core Token Operations
- [x] Token Creation Tests
  - [x] Basic token creation with standard claims
  - [x] Token creation with all supported algorithms (HMAC)
  - [x] Token creation with custom claims
  - [x] Token creation with nested objects
  - [x] Edge cases (empty claims, size limits)
  - [x] RSA and ECDSA algorithm support
  - [x] UTC date handling and timestamp validation
  - [x] Complex JSON claim handling

- [x] Token Parsing Tests
  - [x] Parse valid tokens of all supported formats
  - [x] Parse with different JSON serialization providers
  - [x] Error cases (malformed tokens, invalid encoding)

- [x] Token Validation Tests
  - [x] Signature validation for each algorithm
  - [x] Expiration validation
  - [x] Issuer and audience validation
  - [x] JTI validation
  - [x] Clock skew scenarios
  - [x] Validation method parameter handling

### Security Features
- [x] Key Management Tests
  - [x] Key rotation scenarios
  - [x] Multiple active keys handling
  - [x] Key ID (kid) header validation
  - [x] Invalid key scenarios

- [x] Token Security Tests
  - [x] Replay attack prevention
  - [x] Token substitution prevention
  - [x] Signature tampering detection
  - [x] Claim tampering detection

### RFC Compliance
- [x] Core RFC Compliance Tests
  - [x] RFC 7519 (JWT) compliance
  - [x] RFC 7515 (JWS) compliance
  - [x] IANA registered claim handling
  - [x] Standard claim format compliance

## 🟡 High Priority Tests

### Error Handling
- [x] Validation Error Tests
  - [x] All validation error codes
  - [x] Error message clarity
  - [x] Multiple error aggregation
  - [x] Async operation error handling

### Integration Tests
- [x] JSON Provider Integration
  - [x] System.Text.Json integration
  - [x] Newtonsoft.Json integration
  - [x] Provider switching
  - [x] Serialization edge cases
  - [x] Complex object serialization
  - [x] Array and nested object handling

### Token Revocation
- [x] Revocation System Tests
  - [x] Immediate revocation
  - [x] Delayed revocation
  - [x] Revocation with reason
  - [x] Revocation checks during validation

### Cache Expiration Policy Refactor
- [ ] Cache Refactoring
  - [ ] Refactor InMemoryTokenCache to support configurable eviction policies (LRU, FIFO, etc.)
    - Refactor the internal data structure to efficiently support both LRU and FIFO eviction.
    - Ensure the cache can be easily extended for additional policies in the future.
  - [ ] Implement LRU (Least Recently Used) eviction as the default policy
    - When the cache exceeds its size, evict the least recently used token.
    - Update token access to refresh its usage order.
  - [ ] Allow users to select eviction policy via configuration/constructor
    - Add an enum (e.g., CacheEvictionPolicy) and expose it via constructor or configuration.
    - Document how to set the eviction policy in the README.
  - [ ] Ensure thread-safe and deterministic eviction logic
    - All cache operations (add, update, get, evict) must be thread-safe.
    - Write tests to ensure deterministic eviction behavior under concurrency.
  - [ ] Update all cache-related tests for deterministic eviction
    - Refactor tests to expect LRU or FIFO behavior, not random eviction.
    - Add new tests for edge cases (e.g., repeated access, eviction order).
- [ ] Document new cache configuration options in README
  - Provide usage examples for each eviction policy.
  - Clearly explain default behavior and customization options.
- [ ] Add tests for each eviction policy (LRU, FIFO)
  - Ensure both policies are tested for correctness and concurrency.
  - Include tests for boundary conditions (full cache, empty cache, rapid access).
- [ ] Maintain backward compatibility with existing cache usage
  - Ensure that existing code using InMemoryTokenCache without specifying a policy continues to work as before (default to LRU).
  - Add tests to verify backward compatibility.

## 🟢 Medium Priority Tests

### Performance Tests
- [x] Token Processing Performance
  - [x] Token creation benchmarks
  - [x] Token parsing benchmarks
  - [x] Token validation benchmarks
  - [x] Memory allocation patterns

### Caching System
- [x] Cache Implementation Tests
  - [x] Cache hit/miss scenarios
  - [x] Cache invalidation
  - [x] Cache size limits
  - [x] Concurrent cache access

### Token Repository
- [x] Token Repository Implementation
  - [x] Store issued tokens
  - [x] Query tokens by user ID
  - [x] Proper RevokeAllForUser implementation
  - [x] Repository cleanup (expired tokens)
  - [x] Thread-safe access

### Unity Integration
- [ ] Unity-Specific Tests
  - [ ] Unity serialization
  - [ ] Coroutine-based operations
  - [ ] IL2CPP compatibility
  - [ ] AOT compilation scenarios

## ⚪ Low Priority Tests

### Advanced Features
- [x] Claims Transformation Tests (2025-04-15)
  - [x] Custom claims transformation
  - [x] Role-based permissions
  - [x] Complex claim structures

### Performance Optimization
- [x] Advanced Performance Tests
  - [x] Large token handling (2025-04-16)
  - [x] Batch processing scenarios (2025-04-16)
  - [x] Memory optimization scenarios (2025-04-16)
  - [x] Cache performance impact

### Documentation and Examples
- [ ] Documentation Tests
  - [ ] Example code validation
  - [ ] Documentation accuracy
  - [ ] API usage examples
  - [ ] Best practices validation

## Discovered During Work

### Token Repository and Caching Improvements
- [ ] Add additional test cases for TokenRepositoryRevoker
  - [ ] Test async revocation operations
  - [ ] Test user-based revocation with larger datasets
  - [ ] Test integration with the validation system
- [ ] Add performance benchmarks for repository operations
  - [ ] Measure repository read/write performance under load
  - [ ] Compare performance with different repository sizes
- [ ] Improve cleanup mechanism tests
  - [ ] Test automatic cleanup triggering
  - [ ] Test cleanup with different interval settings
- [ ] Add persistence layer for token repository
  - [ ] Create file-based repository implementation
  - [ ] Add serialization/deserialization for persistent storage

### Flexible Caching Architecture with UniCache Support
- [x] Design provider-agnostic cache abstraction layer
  - [x] Create ICacheProvider and ITokenCacheStorage interfaces
  - [x] Define common cache operations (Get, Set, Remove, Clear)
  - [x] Create serialization abstractions for token persistence
  - [x] Build encryption capability interfaces for secure storage
  - [x] Design configuration mechanism for different providers
- [x] Implement default in-memory cache provider
  - [x] Create SimpleMemoryCacheProvider implementation
  - [x] Support all caching operations with in-memory storage
  - [x] Implement cache eviction policies and size limiting
- [ ] Integrate UniCache as optional provider
  - [x] Create a new SimpleJwt.UniCache project for UniCache support (optional NuGet)
  - [x] Add UniCache and UniCache.Encryption packages to SimpleJwt.UniCache only
  - [x] Implement UniCacheTokenRepository and provider integration in SimpleJwt.UniCache
  - [x] Support both in-memory and persistent storage modes via DI
  - [x] Implement encrypted storage for sensitive token data in UniCache
    - [x] Use AES encryption for all persisted tokens
    - [x] Implement secure key storage mechanism
    - [x] Add salt-based encryption with proper key derivation
    - [x] Create key rotation mechanism for long-term security
  - [x] Create cache expiry policies that align with token lifetimes
- [x] Build extension methods for easy provider registration
  - [x] Add UseInMemoryCache() extension for default provider
  - [x] Add UseUniCache() extension for UniCache integration
  - [x] Add UseCustomCache<T>() for custom provider implementation
  - [x] Create security configuration options for encryption settings
- [x] Create unit tests for caching architecture
  - [x] Test provider-agnostic interfaces with multiple implementations
  - [x] Test in-memory token caching
  - [x] Test persistent token storage with UniCache
  - [x] Test token encryption/decryption
    - [x] Verify encrypted data cannot be read without proper keys
    - [x] Test encryption performance impact
    - [x] Ensure encrypted tokens survive application restarts
  - [x] Test cache expiry and cleanup
  - [x] Test performance impact of caching
- [x] Update documentation
  - [x] Update README with caching architecture information
  - [x] Document UniCache as optional dependency for persistence
  - [x] Add examples for different caching providers
  - [x] Create migration guide for existing implementations
  - [x] Document security best practices for persistent token storage

### Discovered During Work
- [ ] Consider supporting per-token expiration (TTL) as a future enhancement

## Potential Future Tasks

- Persistent Token Blacklist/Whitelist
  - Add support for persistent blacklisting and optional whitelisting of tokens (useful for stateless JWT logout or compromised tokens).
  - Consider file, database, or distributed store backends.
- Token Introspection Endpoint (OAuth2)
  - Implement a standard RFC 7662-compatible token introspection endpoint for OAuth2 resource server compatibility.
- Per-Token TTL and Sliding Expiration
  - Allow setting time-to-live (TTL) for individual tokens in the cache/repository.
  - Optionally support sliding expiration, where TTL is refreshed on access.
- Audit Logging
  - Provide hooks or built-in support for logging token issuance, validation, revocation, and cache evictions.
  - Support integration with standard logging frameworks.
- Distributed Token Repository/Cache
  - Add support for distributed cache/repo backends (e.g., Redis, SQL) for scale-out scenarios.
  - Abstract the storage backend for easy extension.
- Advanced Token Metadata and Querying
  - Allow arbitrary metadata/tags to be associated with tokens.
  - Enable querying tokens by metadata (e.g., device info, IP, client app).
- Advanced Revocation Criteria
  - Support revocation by additional criteria (e.g., by client app, IP, or custom claim).
  - Optionally support scheduled/future revocation.
- Robust Token Replay Detection
  - Ensure replay detection is robust for all token types, especially one-time-use or refresh tokens.
- Admin API/CLI for Token Management
  - Provide an API or CLI for token inspection, revocation, and repository/cache management.
- Extensibility and Customization
  - Ensure all major behaviors (validation, claims transformation, revocation, cache policy) remain pluggable/extensible and are clearly documented.

## Notes
- Tasks should be implemented in order of priority
- Each task should include both happy path and error cases
- All tests should be documented with clear descriptions
- Test coverage should be maintained at >90% for critical and high priority items
- New tasks discovered during implementation should be added to appropriate priority levels

## Progress Tracking
- Total Tasks: 65
- Completed Tasks: 71
- Critical Priority: 24/24
- High Priority: 15/15
- Medium Priority: 20/20
- Low Priority: 4/10

Last Updated: 2025-04-16

## Recent Updates
- Implemented TokenRepository for storing and managing JWT tokens
- Added TokenRepositoryRevoker that integrates with the token repository for better token tracking
- Added thread-safe implementation with automatic cleanup of expired tokens
- Implemented in-memory token caching to improve performance
- Added comprehensive cache implementation with hit/miss tracking, invalidation, size limits, and thread safety
- Fixed concurrency issues in the caching implementation 
- Implemented asynchronous cache eviction to improve performance and prevent blocking
- Added synchronous eviction option for testing scenarios
- Fixed thread safety issues in concurrent cache access testing
- Implemented comprehensive performance testing framework for SimpleJwt
- Added benchmarks for token creation, parsing, and validation with different token sizes and algorithms
- Added memory allocation pattern analysis to identify potential optimization opportunities
- Added concurrent performance testing to ensure library scalability under load
- All performance tests now passing with good results
- Remaining warnings and errors have been resolved 