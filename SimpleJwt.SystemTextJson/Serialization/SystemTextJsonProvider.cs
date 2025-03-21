using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using SimpleJwt.Abstractions.Serialization;

namespace SimpleJwt.SystemTextJson.Serialization
{
    /// <summary>
    /// Implementation of <see cref="IJsonProvider"/> using System.Text.Json.
    /// </summary>
    public class SystemTextJsonProvider : IJsonProvider
    {
        private readonly JsonSerializerOptions _options;
        private readonly HashSet<Type> _registeredTypes = new HashSet<Type>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemTextJsonProvider"/> class with default options.
        /// </summary>
        public SystemTextJsonProvider()
            : this(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemTextJsonProvider"/> class with the specified options.
        /// </summary>
        /// <param name="options">The JSON serializer options.</param>
        public SystemTextJsonProvider(JsonSerializerOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Gets the JSON serializer options being used.
        /// </summary>
        public JsonSerializerOptions Options => _options;

        /// <summary>
        /// Serializes an object to a JSON string.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="value">The object to serialize.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public string Serialize<T>(T value)
        {
            return JsonSerializer.Serialize(value, _options);
        }

        /// <summary>
        /// Serializes an object to a JSON string.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="type">The type of the object to serialize.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public string Serialize(object value, Type type)
        {
            return JsonSerializer.Serialize(value, type, _options);
        }

        /// <summary>
        /// Deserializes a JSON string to an object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        public T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, _options);
        }

        /// <summary>
        /// Deserializes a JSON string to an object of the specified type.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="type">The type of the object to deserialize to.</param>
        /// <returns>The deserialized object.</returns>
        public object Deserialize(string json, Type type)
        {
            return JsonSerializer.Deserialize(json, type, _options);
        }

        /// <summary>
        /// Gets a value indicating whether this provider supports AOT (Ahead-of-Time) compilation.
        /// </summary>
        /// <remarks>
        /// System.Text.Json doesn't require type registration for AOT compilation.
        /// </remarks>
        public bool SupportsAotCompilation => false;

        /// <summary>
        /// Registers a type for AOT serialization/deserialization.
        /// </summary>
        /// <param name="type">The type to register.</param>
        /// <remarks>
        /// This method is a no-op for System.Text.Json as it doesn't require explicit type registration.
        /// </remarks>
        public void RegisterAotType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            
            _registeredTypes.Add(type);
        }

        /// <summary>
        /// Registers a type for AOT serialization/deserialization.
        /// </summary>
        /// <typeparam name="T">The type to register.</typeparam>
        /// <remarks>
        /// This method is a no-op for System.Text.Json as it doesn't require explicit type registration.
        /// </remarks>
        public void RegisterAotType<T>()
        {
            RegisterAotType(typeof(T));
        }
    }
} 