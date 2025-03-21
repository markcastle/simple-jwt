using System;
using System.Collections.Generic;

namespace SimpleJwt.Abstractions.Serialization
{
    /// <summary>
    /// Provides JSON serialization and deserialization services.
    /// </summary>
    public interface IJsonProvider
    {
        /// <summary>
        /// Serializes an object to a JSON string.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="value">The object to serialize.</param>
        /// <returns>A JSON string representation of the object.</returns>
        string Serialize<T>(T value);
        
        /// <summary>
        /// Serializes an object to a JSON string.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="type">The type of the object to serialize.</param>
        /// <returns>A JSON string representation of the object.</returns>
        string Serialize(object value, Type type);
        
        /// <summary>
        /// Deserializes a JSON string to an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        T Deserialize<T>(string json);
        
        /// <summary>
        /// Deserializes a JSON string to an object of the specified type.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="type">The type to deserialize to.</param>
        /// <returns>The deserialized object.</returns>
        object Deserialize(string json, Type type);
        
        /// <summary>
        /// Gets a value indicating whether this provider supports AOT (Ahead-of-Time) compilation.
        /// </summary>
        bool SupportsAotCompilation { get; }
        
        /// <summary>
        /// Registers a type for AOT serialization/deserialization.
        /// </summary>
        /// <param name="type">The type to register.</param>
        void RegisterAotType(Type type);
        
        /// <summary>
        /// Registers a type for AOT serialization/deserialization.
        /// </summary>
        /// <typeparam name="T">The type to register.</typeparam>
        void RegisterAotType<T>();
    }
} 