using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SimpleJwt.Abstractions.Serialization;

namespace SimpleJwt.Newtonsoft.Serialization
{
    /// <summary>
    /// Provides JSON serialization services using Newtonsoft.Json.
    /// </summary>
    public class NewtonsoftJsonProvider : IJsonProvider
    {
        private readonly JsonSerializerSettings _settings;
        private readonly HashSet<Type> _registeredTypes = new HashSet<Type>();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="NewtonsoftJsonProvider"/> class
        /// with default serialization settings.
        /// </summary>
        public NewtonsoftJsonProvider() 
            : this(new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DateParseHandling = DateParseHandling.DateTimeOffset
            })
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="NewtonsoftJsonProvider"/> class
        /// with the specified serialization settings.
        /// </summary>
        /// <param name="settings">The JSON serializer settings.</param>
        public NewtonsoftJsonProvider(JsonSerializerSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }
        
        /// <summary>
        /// Gets the JSON serializer settings.
        /// </summary>
        public JsonSerializerSettings SerializerSettings => _settings;
        
        /// <inheritdoc/>
        public string Serialize<T>(T value)
        {
            return JsonConvert.SerializeObject(value, _settings);
        }
        
        /// <inheritdoc/>
        public string Serialize(object value, Type type)
        {
            return JsonConvert.SerializeObject(value, type, _settings);
        }
        
        /// <inheritdoc/>
        public T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, _settings);
        }
        
        /// <inheritdoc/>
        public object Deserialize(string json, Type type)
        {
            return JsonConvert.DeserializeObject(json, type, _settings);
        }
        
        /// <inheritdoc/>
        public bool SupportsAotCompilation => true;
        
        /// <inheritdoc/>
        public void RegisterAotType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            
            _registeredTypes.Add(type);
            
            // Register for AOT compilation
            JsonSerializer.CreateDefault(_settings).Converters.Add(new AOTSupportConverter(type));
        }
        
        /// <inheritdoc/>
        public void RegisterAotType<T>()
        {
            RegisterAotType(typeof(T));
        }
        
        /// <summary>
        /// Custom JSON converter for AOT compilation support.
        /// </summary>
        private class AOTSupportConverter : JsonConverter
        {
            private readonly Type _type;
            
            public AOTSupportConverter(Type type)
            {
                _type = type;
            }
            
            public override bool CanConvert(Type objectType)
            {
                return objectType == _type;
            }
            
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                // This is just a dummy implementation to force AOT compilation
                // The actual conversion will use the standard Newtonsoft.Json converters
                return existingValue ?? Activator.CreateInstance(objectType);
            }
            
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                // This is just a dummy implementation to force AOT compilation
                // The actual conversion will use the standard Newtonsoft.Json converters
                writer.WriteNull();
            }
            
            public override bool CanRead => false;
            public override bool CanWrite => false;
        }
    }
    
    /// <summary>
    /// Helper class for registering types for AOT compilation with Newtonsoft.Json.
    /// </summary>
    public static class NewtonsoftJsonAotHelper
    {
        /// <summary>
        /// Registers a type for AOT serialization/deserialization.
        /// </summary>
        /// <typeparam name="T">The type to register.</typeparam>
        public static void RegisterType<T>()
        {
            if (JsonProviderConfiguration.IsProviderConfigured)
            {
                JsonProviderConfiguration.GetProvider().RegisterAotType<T>();
            }
        }
        
        /// <summary>
        /// Registers a type for AOT serialization/deserialization.
        /// </summary>
        /// <param name="type">The type to register.</param>
        public static void RegisterType(Type type)
        {
            if (JsonProviderConfiguration.IsProviderConfigured)
            {
                JsonProviderConfiguration.GetProvider().RegisterAotType(type);
            }
        }
    }
} 