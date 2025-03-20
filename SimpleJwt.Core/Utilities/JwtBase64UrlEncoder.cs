using System;
using System.Text;

namespace SimpleJwt.Core.Utilities
{
    /// <summary>
    /// Provides methods for encoding and decoding Base64Url strings used in JWT tokens.
    /// </summary>
    internal static class JwtBase64UrlEncoder
    {
        /// <summary>
        /// Encodes the specified byte array to a Base64Url string.
        /// </summary>
        /// <param name="input">The byte array to encode.</param>
        /// <returns>The Base64Url encoded string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
        public static string Encode(byte[] input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (input.Length == 0)
            {
                return string.Empty;
            }

            // Convert to base64 first
            string base64 = Convert.ToBase64String(input);
            
            // Then convert to base64url by replacing characters that are different in base64url
            return base64
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        /// <summary>
        /// Encodes the specified string to a Base64Url string using UTF-8 encoding.
        /// </summary>
        /// <param name="input">The string to encode.</param>
        /// <returns>The Base64Url encoded string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
        public static string Encode(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (input.Length == 0)
            {
                return string.Empty;
            }

            byte[] bytes = Encoding.UTF8.GetBytes(input);
            return Encode(bytes);
        }

        /// <summary>
        /// Decodes the specified Base64Url string to a byte array.
        /// </summary>
        /// <param name="input">The Base64Url encoded string to decode.</param>
        /// <returns>The decoded byte array.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
        /// <exception cref="FormatException">Thrown when <paramref name="input"/> is not a valid Base64Url string.</exception>
        public static byte[] DecodeToBytes(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (input.Length == 0)
            {
                return Array.Empty<byte>();
            }

            string base64 = input
                .Replace('-', '+')
                .Replace('_', '/');

            // Add padding if needed
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }

            return Convert.FromBase64String(base64);
        }

        /// <summary>
        /// Decodes the specified Base64Url string to a string using UTF-8 encoding.
        /// </summary>
        /// <param name="input">The Base64Url encoded string to decode.</param>
        /// <returns>The decoded string.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
        /// <exception cref="FormatException">Thrown when <paramref name="input"/> is not a valid Base64Url string.</exception>
        public static string Decode(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (input.Length == 0)
            {
                return string.Empty;
            }

            byte[] bytes = DecodeToBytes(input);
            return Encoding.UTF8.GetString(bytes);
        }
    }
} 