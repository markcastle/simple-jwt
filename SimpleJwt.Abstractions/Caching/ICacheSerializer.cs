using System;

namespace SimpleJwt.Abstractions.Caching
{
    /// <summary>
    /// Abstraction for serializing and deserializing cache values.
    /// </summary>
    public interface ICacheSerializer
    {
        /// <summary>
        /// Serializes a value to a string.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="value">The value to serialize.</param>
        /// <returns>The serialized string.</returns>
        string Serialize<T>(T value);

        /// <summary>
        /// Deserializes a string to a value.
        /// </summary>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <param name="serialized">The serialized string.</param>
        /// <returns>The deserialized value.</returns>
        T Deserialize<T>(string serialized);
    }
}
