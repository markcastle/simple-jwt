using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Threading.Tasks;
using SimpleJwt.Abstractions;
using SimpleJwt.Abstractions.Validation;
using SimpleJwt.Core;
using SimpleJwt.Core.Validation;
using Xunit;
using Xunit.Abstractions;

namespace SimpleJwt.Tests
{
    /// <summary>
    /// Performance tests for the SimpleJwt library.
    /// </summary>
    public class JwtTokenPerformanceTests : TestBase
    {
        private readonly IJwtBuilder _builder;
        private readonly IJwtParser _parser;
        private readonly IJwtValidator _validator;
        private readonly ITestOutputHelper _output;
        private readonly byte[] _hmacKey;
        private readonly RSA _rsaKey;
        private readonly ECDsa _ecdsaKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="JwtTokenPerformanceTests"/> class.
        /// </summary>
        /// <param name="output">The test output helper to write results to.</param>
        public JwtTokenPerformanceTests(ITestOutputHelper output) : base(useSystemTextJson: true)
        {
            _output = output;
            _builder = new JwtBuilder();
            _parser = new JwtParser();
            _validator = new JwtValidator(_parser);
            
            // Create test keys
            _hmacKey = new byte[32];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(_hmacKey);
            }
            
            _rsaKey = RSA.Create(2048);
            _ecdsaKey = ECDsa.Create();
        }

        /// <summary>
        /// Tests the performance of token creation with different claims.
        /// </summary>
        [Fact]
        public void ShouldMeasureTokenCreationPerformance()
        {
            // Define test scenarios with different payload sizes
            var scenarios = new (string Name, Func<string> CreateTokenFunc)[]
            {
                ("Small Token (5 claims)", CreateSmallToken),
                ("Medium Token (20 claims)", CreateMediumToken),
                ("Large Token (100 claims)", CreateLargeToken)
            };

            // Run each scenario and measure performance
            foreach (var scenario in scenarios)
            {
                string name = scenario.Name;
                Func<string> createTokenFunc = scenario.CreateTokenFunc;
                
                // Warm-up
                for (int i = 0; i < 10; i++)
                {
                    createTokenFunc();
                }

                // Measure
                const int iterations = 1000;
                var watch = Stopwatch.StartNew();
                
                for (int i = 0; i < iterations; i++)
                {
                    createTokenFunc();
                }
                
                watch.Stop();
                
                double averageMs = watch.ElapsedMilliseconds / (double)iterations;
                double tokensPerSecond = iterations * 1000.0 / watch.ElapsedMilliseconds;
                
                _output.WriteLine($"{name}: {averageMs:F3} ms per token ({tokensPerSecond:F0} tokens/second)");
            }
        }

        /// <summary>
        /// Tests the performance of token parsing with different sizes.
        /// </summary>
        [Fact]
        public void ShouldMeasureTokenParsingPerformance()
        {
            // Create tokens of different sizes
            string smallToken = CreateSmallToken();
            string mediumToken = CreateMediumToken();
            string largeToken = CreateLargeToken();

            var scenarios = new (string Name, string Token)[]
            {
                ("Small Token Parse", smallToken),
                ("Medium Token Parse", mediumToken),
                ("Large Token Parse", largeToken)
            };

            // Run each scenario and measure performance
            foreach (var scenario in scenarios)
            {
                string name = scenario.Name;
                string token = scenario.Token;
                
                // Warm-up
                for (int i = 0; i < 10; i++)
                {
                    _parser.Parse(token);
                }

                // Measure
                const int iterations = 1000;
                var watch = Stopwatch.StartNew();
                
                for (int i = 0; i < iterations; i++)
                {
                    _parser.Parse(token);
                }
                
                watch.Stop();
                
                double averageMs = watch.ElapsedMilliseconds / (double)iterations;
                double tokensPerSecond = iterations * 1000.0 / watch.ElapsedMilliseconds;
                
                _output.WriteLine($"{name}: {averageMs:F3} ms per token ({tokensPerSecond:F0} tokens/second)");
            }
        }

        /// <summary>
        /// Tests the performance of token validation with different algorithms.
        /// </summary>
        [Fact]
        public void ShouldMeasureTokenValidationPerformance()
        {
            // Create tokens with different signing algorithms
            string hs256Token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(30))
                .SetIssuedNow()
                .SignHs256(_hmacKey);

            string rs256Token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(30))
                .SetIssuedNow()
                .SignRs256(_rsaKey);

            string es256Token = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(30))
                .SetIssuedNow()
                .SignEs256(_ecdsaKey);

            var scenarios = new (string Name, string Token, ValidationParameters Parameters)[]
            {
                ("HS256 Validation", hs256Token, new ValidationParameters { SymmetricSecurityKey = _hmacKey }),
                ("RS256 Validation", rs256Token, new ValidationParameters { RsaSecurityKey = _rsaKey }),
                ("ES256 Validation", es256Token, new ValidationParameters { EcdsaSecurityKey = _ecdsaKey })
            };

            // Run each scenario and measure performance
            foreach (var scenario in scenarios)
            {
                string name = scenario.Name;
                string token = scenario.Token;
                ValidationParameters parameters = scenario.Parameters;
                
                // Parse token once for validation tests
                IJwtToken parsedToken = _parser.Parse(token);

                // Warm-up
                for (int i = 0; i < 10; i++)
                {
                    _validator.Validate(parsedToken, parameters);
                }

                // Measure
                const int iterations = 1000;
                var watch = Stopwatch.StartNew();
                
                for (int i = 0; i < iterations; i++)
                {
                    _validator.Validate(parsedToken, parameters);
                }
                
                watch.Stop();
                
                double averageMs = watch.ElapsedMilliseconds / (double)iterations;
                double validationsPerSecond = iterations * 1000.0 / watch.ElapsedMilliseconds;
                
                _output.WriteLine($"{name}: {averageMs:F3} ms per validation ({validationsPerSecond:F0} validations/second)");
            }
        }

        /// <summary>
        /// Tests the memory allocation patterns during token operations.
        /// </summary>
        [Fact]
        public void ShouldMeasureMemoryAllocationPatterns()
        {
            // Measure memory allocation for token creation
            long beforeCreation = GC.GetTotalMemory(true);
            const int creationIterations = 1000;
            
            List<string> tokens = new List<string>(creationIterations);
            for (int i = 0; i < creationIterations; i++)
            {
                tokens.Add(CreateSmallToken());
            }
            
            long afterCreation = GC.GetTotalMemory(false);
            long creationMemory = afterCreation - beforeCreation;
            double perTokenCreationBytes = creationMemory / (double)creationIterations;
            
            _output.WriteLine($"Token Creation: ~{perTokenCreationBytes:F0} bytes per token");

            // Measure memory allocation for token parsing
            GC.Collect();
            long beforeParsing = GC.GetTotalMemory(true);
            
            List<IJwtToken> parsedTokens = new List<IJwtToken>(creationIterations);
            foreach (string token in tokens)
            {
                parsedTokens.Add(_parser.Parse(token));
            }
            
            long afterParsing = GC.GetTotalMemory(false);
            long parsingMemory = afterParsing - beforeParsing;
            double perTokenParsingBytes = parsingMemory / (double)creationIterations;
            
            _output.WriteLine($"Token Parsing: ~{perTokenParsingBytes:F0} bytes per token");

            // Measure memory allocation for token validation
            GC.Collect();
            long beforeValidation = GC.GetTotalMemory(true);
            
            var parameters = new ValidationParameters { SymmetricSecurityKey = _hmacKey };
            foreach (IJwtToken token in parsedTokens)
            {
                _validator.Validate(token, parameters);
            }
            
            long afterValidation = GC.GetTotalMemory(false);
            long validationMemory = afterValidation - beforeValidation;
            double perTokenValidationBytes = validationMemory / (double)creationIterations;
            
            _output.WriteLine($"Token Validation: ~{perTokenValidationBytes:F0} bytes per validation");
        }

        /// <summary>
        /// Tests the performance of concurrent token operations.
        /// </summary>
        [Fact]
        public async Task ShouldMeasureConcurrentOperationsPerformance()
        {
            const int concurrentOperations = 100;
            const int iterationsPerTask = 100;
            
            // Create token once for validation tests
            string token = CreateSmallToken();
            IJwtToken parsedToken = _parser.Parse(token);
            var parameters = new ValidationParameters { SymmetricSecurityKey = _hmacKey };
            
            // Measure concurrent creation
            var creationWatch = Stopwatch.StartNew();
            
            var creationTasks = new List<Task>(concurrentOperations);
            for (int i = 0; i < concurrentOperations; i++)
            {
                creationTasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < iterationsPerTask; j++)
                    {
                        CreateSmallToken();
                    }
                }));
            }
            
            await Task.WhenAll(creationTasks);
            creationWatch.Stop();
            
            double creationThroughput = concurrentOperations * iterationsPerTask * 1000.0 / creationWatch.ElapsedMilliseconds;
            _output.WriteLine($"Concurrent Creation: {creationThroughput:F0} tokens/second");
            
            // Measure concurrent parsing
            var parsingWatch = Stopwatch.StartNew();
            
            var parsingTasks = new List<Task>(concurrentOperations);
            for (int i = 0; i < concurrentOperations; i++)
            {
                parsingTasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < iterationsPerTask; j++)
                    {
                        _parser.Parse(token);
                    }
                }));
            }
            
            await Task.WhenAll(parsingTasks);
            parsingWatch.Stop();
            
            double parsingThroughput = concurrentOperations * iterationsPerTask * 1000.0 / parsingWatch.ElapsedMilliseconds;
            _output.WriteLine($"Concurrent Parsing: {parsingThroughput:F0} tokens/second");
            
            // Measure concurrent validation
            var validationWatch = Stopwatch.StartNew();
            
            var validationTasks = new List<Task>(concurrentOperations);
            for (int i = 0; i < concurrentOperations; i++)
            {
                validationTasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < iterationsPerTask; j++)
                    {
                        _validator.Validate(parsedToken, parameters);
                    }
                }));
            }
            
            await Task.WhenAll(validationTasks);
            validationWatch.Stop();
            
            double validationThroughput = concurrentOperations * iterationsPerTask * 1000.0 / validationWatch.ElapsedMilliseconds;
            _output.WriteLine($"Concurrent Validation: {validationThroughput:F0} validations/second");
        }

        #region Helper Methods

        private string CreateSmallToken()
        {
            return _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(30))
                .SetIssuedNow()
                .SignHs256(_hmacKey);
        }

        private string CreateMediumToken()
        {
            var builder = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(30))
                .SetIssuedNow();

            // Add additional claims to make a medium-sized token
            for (int i = 0; i < 15; i++)
            {
                builder = builder.AddClaim($"claim{i}", $"value{i}");
            }

            return builder.SignHs256(_hmacKey);
        }

        private string CreateLargeToken()
        {
            var builder = _builder
                .SetIssuer("test-issuer")
                .SetAudience("test-audience")
                .SetSubject("test-subject")
                .SetExpiration(TimeSpan.FromMinutes(30))
                .SetIssuedNow();

            // Add additional claims to make a large token
            for (int i = 0; i < 95; i++)
            {
                builder = builder.AddClaim($"claim{i}", $"value{i}");
            }

            return builder.SignHs256(_hmacKey);
        }

        #endregion
    }
} 