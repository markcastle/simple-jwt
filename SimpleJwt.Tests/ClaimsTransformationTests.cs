using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SimpleJwt.Abstractions;
using SimpleJwt.Abstractions.Validation;
using SimpleJwt.Core;
using SimpleJwt.Core.Validation;
using Xunit;

namespace SimpleJwt.Tests
{
    /// <summary>
    /// Unit tests for claims transformation logic, covering custom claims, role-based permissions, and complex claim structures.
    /// </summary>
    public class ClaimsTransformationTests
    {
        private readonly IJwtBuilder _jwtBuilder;
        private readonly IJwtParser _jwtParser;
        private readonly byte[] _hmacKey;

        public ClaimsTransformationTests()
        {
            _jwtBuilder = new JwtBuilder();
            _jwtParser = new JwtParser();
            _hmacKey = System.Text.Encoding.UTF8.GetBytes("super-secret-key-which-is-32-bytes!");
        }

        private static ValidationContext CreateDummyValidationContext()
        {
            // Dummy token and parameters for ValidationContext constructor
            var dummyToken = new DummyJwtToken();
            var dummyParams = new ValidationParameters();
            return new ValidationContext(dummyToken, dummyParams);
        }

        /// <summary>
        /// Tests that a custom claims transformer correctly adds a custom claim.
        /// </summary>
        [Fact]
        public void ShouldAddCustomClaimDuringTransformation()
        {
            // Arrange
            var originalClaims = new Dictionary<string, object> { { "sub", "user1" } };
            var transformer = new CustomClaimAdderTransformer();
            var context = CreateDummyValidationContext();

            // Act
            var transformed = transformer.TransformClaims(originalClaims, context);

            // Assert
            Assert.True(transformed.ContainsKey("custom"));
            Assert.Equal("custom-value", transformed["custom"]);
        }

        /// <summary>
        /// Tests that a role-based transformer enforces role claims correctly.
        /// </summary>
        [Fact]
        public void ShouldTransformRolesCorrectly()
        {
            // Arrange
            var originalClaims = new Dictionary<string, object> { { "roles", new[] { "admin", "user" } } };
            var transformer = new RoleBasedTransformer();
            var context = CreateDummyValidationContext();

            // Act
            var transformed = transformer.TransformClaims(originalClaims, context);

            // Assert
            Assert.True(transformed.ContainsKey("isAdmin"));
            Assert.True((bool)transformed["isAdmin"]);
        }

        /// <summary>
        /// Tests complex claim structure transformation.
        /// </summary>
        [Fact]
        public void ShouldHandleComplexClaimStructures()
        {
            // Arrange
            var originalClaims = new Dictionary<string, object>
            {
                { "profile", new Dictionary<string, object> { { "age", 30 }, { "country", "UK" } } }
            };
            var transformer = new ComplexStructureTransformer();
            var context = CreateDummyValidationContext();

            // Act
            var transformed = transformer.TransformClaims(originalClaims, context);

            // Assert
            Assert.True(transformed.ContainsKey("isAdult"));
            Assert.True((bool)transformed["isAdult"]);
        }

        /// <summary>
        /// Tests asynchronous claim transformation.
        /// </summary>
        [Fact]
        public async Task ShouldTransformClaimsAsync()
        {
            // Arrange
            var originalClaims = new Dictionary<string, object> { { "sub", "user1" } };
            var transformer = new CustomClaimAdderTransformer();
            var context = CreateDummyValidationContext();

            // Act
            var transformed = await transformer.TransformClaimsAsync(originalClaims, context, CancellationToken.None);

            // Assert
            Assert.True(transformed.ContainsKey("custom"));
            Assert.Equal("custom-value", transformed["custom"]);
        }

        // --- Helper transformers for testing ---
        private class CustomClaimAdderTransformer : IClaimsTransformer
        {
            public IDictionary<string, object> TransformClaims(IDictionary<string, object> claims, ValidationContext context)
            {
                var result = new Dictionary<string, object>(claims);
                result["custom"] = "custom-value";
                return result;
            }

            public Task<IDictionary<string, object>> TransformClaimsAsync(IDictionary<string, object> claims, ValidationContext context, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(TransformClaims(claims, context));
            }
        }

        private class RoleBasedTransformer : IClaimsTransformer
        {
            public IDictionary<string, object> TransformClaims(IDictionary<string, object> claims, ValidationContext context)
            {
                var result = new Dictionary<string, object>(claims);
                if (claims.TryGetValue("roles", out var rolesObj) && rolesObj is IEnumerable<string> roles)
                {
                    result["isAdmin"] = System.Linq.Enumerable.Contains(roles, "admin");
                }
                else
                {
                    result["isAdmin"] = false;
                }
                return result;
            }

            public Task<IDictionary<string, object>> TransformClaimsAsync(IDictionary<string, object> claims, ValidationContext context, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(TransformClaims(claims, context));
            }
        }

        private class ComplexStructureTransformer : IClaimsTransformer
        {
            public IDictionary<string, object> TransformClaims(IDictionary<string, object> claims, ValidationContext context)
            {
                var result = new Dictionary<string, object>(claims);
                if (claims.TryGetValue("profile", out var profileObj) && profileObj is IDictionary<string, object> profile)
                {
                    if (profile.TryGetValue("age", out var ageObj) && ageObj is int age)
                    {
                        result["isAdult"] = age >= 18;
                    }
                }
                return result;
            }

            public Task<IDictionary<string, object>> TransformClaimsAsync(IDictionary<string, object> claims, ValidationContext context, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(TransformClaims(claims, context));
            }
        }

        // Dummy IJwtToken implementation for ValidationContext
        private class DummyJwtToken : IJwtToken
        {
            public string RawToken => string.Empty;
            public IReadOnlyDictionary<string, object> Header => new Dictionary<string, object>();
            public IReadOnlyDictionary<string, object> Payload => new Dictionary<string, object>();
            public string Signature => string.Empty;
            public bool TryGetHeaderClaim<T>(string name, out T value) { value = default; return false; }
            public bool TryGetClaim<T>(string name, out T value) { value = default; return false; }
            public T GetClaim<T>(string claimName) => default;
            public IJwtToken WithClaim(string name, object value) => this;
            public IJwtToken WithoutClaim(string name) => this;
            public IJwtToken WithHeaderClaim(string name, object value) => this;
            public IJwtToken WithoutHeaderClaim(string name) => this;
        }
    }
}
