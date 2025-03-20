using System;
using System.Collections.Generic;

namespace SimpleJwt.Abstractions.Validation
{
    /// <summary>
    /// Provides context for JWT token validation operations.
    /// </summary>
    public class ValidationContext
    {
        private readonly Dictionary<string, object> _items = new Dictionary<string, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationContext"/> class.
        /// </summary>
        /// <param name="token">The JWT token being validated.</param>
        /// <param name="parameters">The validation parameters.</param>
        public ValidationContext(IJwtToken token, ValidationParameters parameters)
        {
            Token = token ?? throw new ArgumentNullException(nameof(token));
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            ValidationTime = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Gets the JWT token being validated.
        /// </summary>
        public IJwtToken Token { get; }

        /// <summary>
        /// Gets the validation parameters.
        /// </summary>
        public ValidationParameters Parameters { get; }

        /// <summary>
        /// Gets the time at which validation is being performed.
        /// </summary>
        public DateTimeOffset ValidationTime { get; }

        /// <summary>
        /// Gets a collection of items associated with the validation context.
        /// </summary>
        public IReadOnlyDictionary<string, object> Items => _items;

        /// <summary>
        /// Gets or sets an item in the validation context by key.
        /// </summary>
        /// <param name="key">The key of the item.</param>
        /// <returns>The item with the specified key, or null if not found.</returns>
        public object this[string key]
        {
            get => _items.TryGetValue(key, out var value) ? value : null;
            set => _items[key] = value;
        }

        /// <summary>
        /// Adds an item to the validation context.
        /// </summary>
        /// <param name="key">The key of the item.</param>
        /// <param name="value">The value of the item.</param>
        public void AddItem(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));
            }

            _items[key] = value;
        }

        /// <summary>
        /// Removes an item from the validation context.
        /// </summary>
        /// <param name="key">The key of the item to remove.</param>
        /// <returns>true if the item was removed; otherwise, false.</returns>
        public bool RemoveItem(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty.", nameof(key));
            }

            return _items.Remove(key);
        }

        /// <summary>
        /// Gets an item from the validation context.
        /// </summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="key">The key of the item.</param>
        /// <returns>The item with the specified key, or default(T) if not found.</returns>
        public T GetItem<T>(string key)
        {
            if (_items.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }

            return default;
        }

        /// <summary>
        /// Tries to get an item from the validation context.
        /// </summary>
        /// <typeparam name="T">The type of the item.</typeparam>
        /// <param name="key">The key of the item.</param>
        /// <param name="value">When this method returns, contains the value of the item if found; otherwise, the default value for the type of the value parameter.</param>
        /// <returns>true if the item was found; otherwise, false.</returns>
        public bool TryGetItem<T>(string key, out T value)
        {
            if (_items.TryGetValue(key, out var item) && item is T typedValue)
            {
                value = typedValue;
                return true;
            }

            value = default;
            return false;
        }
    }
} 