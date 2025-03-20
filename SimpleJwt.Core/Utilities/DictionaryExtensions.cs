using System;
using System.Collections.Generic;

namespace SimpleJwt.Core.Utilities
{
    /// <summary>
    /// Extension methods for dictionaries.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Gets the string value for the specified key, or returns the default value if the key doesn't exist or the value is null.
        /// </summary>
        /// <param name="dictionary">The dictionary to retrieve the value from.</param>
        /// <param name="key">The key to look up.</param>
        /// <param name="defaultValue">The default value to return if the key doesn't exist or the value is null.</param>
        /// <returns>The string value for the specified key, or the default value if the key doesn't exist or the value is null.</returns>
        public static string GetStringOrDefault(this IReadOnlyDictionary<string, object> dictionary, string key, string defaultValue)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));
            }

            if (dictionary.TryGetValue(key, out object value))
            {
                return value?.ToString() ?? defaultValue;
            }

            return defaultValue;
        }
    }
} 