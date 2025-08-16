using Newtonsoft.Json;
using System;
using JaLoaderClassic;
using Console = JaLoaderClassic.Console;

namespace UnityEngine
{
    /// <summary>
    /// Unity JsonUtility wrapper that uses Newtonsoft.Json for serialization and deserialization.
    /// </summary>
    public class JsonUtility
    {
        private static readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            Converters = { new Vector3Converter() }
        };

        /// <summary>
        /// Deserializes a JSON string into an object of a specified type.
        /// Works similarly to JsonUtility.FromJson.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>An object of type T populated with data from the JSON string.</returns>
        public static T FromJson<T>(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json, _serializerSettings);
            }
            catch (Exception ex)
            {
                Console.InternalLogError("JaLoader", $"Error deserializing JSON: {ex.Message}");
                return default(T);
            }
        }

        /// <summary>
        /// Serializes an object to a JSON string.
        /// Works similarly to JsonUtility.ToJson, with a pretty print option.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        /// <param name="prettyPrint">If true, formats the JSON string with indentation.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string ToJson(object obj, bool prettyPrint = false)
        {
            try
            {
                if (prettyPrint)
                {
                    return JsonConvert.SerializeObject(obj, Formatting.Indented, _serializerSettings);
                }
                else
                {
                    return JsonConvert.SerializeObject(obj, _serializerSettings);
                }
            }
            catch (Exception ex)
            {
                Console.InternalLogError("JaLoader", $"Error serializing object to JSON: {ex.Message}");
                return null;
            }
        }
    }

    public class Vector3Converter : JsonConverter<Vector3>
    {
        public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(value.x);
            writer.WritePropertyName("y");
            writer.WriteValue(value.y);
            writer.WritePropertyName("z");
            writer.WriteValue(value.z);
            writer.WriteEndObject();
        }

        public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return existingValue;
            }

            if (reader.TokenType != JsonToken.StartObject)
            {
                throw new JsonSerializationException("Expected StartObject token when deserializing Vector3.");
            }

            float x = 0, y = 0, z = 0;

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                {
                    return new Vector3(x, y, z);
                }

                if (reader.TokenType != JsonToken.PropertyName)
                {
                    throw new JsonSerializationException("Expected PropertyName token.");
                }

                string propertyName = reader.Value.ToString();
                reader.Read();

                switch (propertyName.ToLowerInvariant())
                {
                    case "x":
                        x = serializer.Deserialize<float>(reader);
                        break;
                    case "y":
                        y = serializer.Deserialize<float>(reader);
                        break;
                    case "z":
                        z = serializer.Deserialize<float>(reader);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            throw new JsonSerializationException("Unexpected end of JSON string.");
        }

        public override bool CanWrite => true;
    }
}
