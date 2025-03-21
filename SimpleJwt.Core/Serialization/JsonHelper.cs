using System;
using SimpleJwt.Abstractions.Serialization;

namespace SimpleJwt.Core.Serialization
{
    /// <summary>
    /// Provides helper methods for JSON serialization and deserialization.
    /// </summary>
    internal static class JsonHelper
    {
        /// <summary>
        /// Serializes an object to a JSON string.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="value">The object to serialize.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string Serialize<T>(T value)
        {
            return JsonProviderConfiguration.GetProvider().Serialize(value);
        }
        
        /// <summary>
        /// Serializes an object to a JSON string.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="type">The type of the object to serialize.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string Serialize(object value, Type type)
        {
            return JsonProviderConfiguration.GetProvider().Serialize(value, type);
        }
        
        /// <summary>
        /// Deserializes a JSON string to an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        public static T Deserialize<T>(string json)
        {
            return JsonProviderConfiguration.GetProvider().Deserialize<T>(json);
        }
        
        /// <summary>
        /// Deserializes a JSON string to an object of the specified type.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="type">The type to deserialize to.</param>
        /// <returns>The deserialized object.</returns>
        public static object Deserialize(string json, Type type)
        {
            return JsonProviderConfiguration.GetProvider().Deserialize(json, type);
        }
        
        /// <summary>
        /// Tries to deserialize a JSON string to an object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to deserialize to.</typeparam>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="result">When this method returns, contains the deserialized object if successful, or the default value if unsuccessful.</param>
        /// <returns>true if deserialization was successful; otherwise, false.</returns>
        public static bool TryDeserialize<T>(string json, out T result)
        {
            try
            {
                result = Deserialize<T>(json);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }
        
        /// <summary>
        /// Tries to deserialize a JSON string to an object of the specified type.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="type">The type to deserialize to.</param>
        /// <param name="result">When this method returns, contains the deserialized object if successful, or null if unsuccessful.</param>
        /// <returns>true if deserialization was successful; otherwise, false.</returns>
        public static bool TryDeserialize(string json, Type type, out object result)
        {
            try
            {
                result = Deserialize(json, type);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }
} 