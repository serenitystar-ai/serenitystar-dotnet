using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SerenityStar.Models.SpeechGeneration
{
    /// <summary>
    /// JSON converter for StreamingSpeechGenerationMessage that handles polymorphic deserialization.
    /// </summary>
    internal class StreamingSpeechGenerationMessageJsonConverter : JsonConverter<StreamingSpeechGenerationMessage>
    {
        /// <summary>
        /// Options with SnakeCaseLower naming policy for speech streaming messages with this converter.
        /// </summary>
        /// <remarks>
        /// Cached here instead of JsonSerializerOptionsCache because SerenityStar.Core must not know AI service types.
        /// Read why caching is needed: "CA1869: Cache and reuse 'JsonSerializerOptions' instances"
        /// https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1869
        /// </remarks>
        internal static readonly JsonSerializerOptions s_options = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            Converters = { new StreamingSpeechGenerationMessageJsonConverter() }
        };

        public override StreamingSpeechGenerationMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using JsonDocument document = JsonDocument.ParseValue(ref reader);
            JsonElement jsonObject = document.RootElement;

            if (!jsonObject.TryGetProperty("type", out JsonElement typeElement))
            {
                throw new JsonException("Missing 'type' property in streaming message");
            }

            string? type = typeElement.ValueKind == JsonValueKind.String ? typeElement.GetString() : null;

            if (type == null)
            {
                throw new JsonException("'type' property must be a non-null string");
            }

            StreamingSpeechGenerationMessage message = type switch
            {
                "start" => JsonSerializer.Deserialize<StreamingSpeechGenerationMessageStart>(jsonObject, options),
                "audio" => JsonSerializer.Deserialize<StreamingSpeechGenerationMessageAudio>(jsonObject, options),
                "ping" => JsonSerializer.Deserialize<StreamingSpeechGenerationMessagePing>(jsonObject, options),
                "error" => JsonSerializer.Deserialize<StreamingSpeechGenerationMessageError>(jsonObject, options),
                "stop" => JsonSerializer.Deserialize<StreamingSpeechGenerationMessageStop>(jsonObject, options),
                _ => CreateUnsupportedMessage(type, jsonObject)
            } ?? throw new JsonException($"Failed to deserialize streaming message of type '{type}'");

            return message;
        }

        public override void Write(Utf8JsonWriter writer, StreamingSpeechGenerationMessage value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }

        private static StreamingSpeechGenerationMessage CreateUnsupportedMessage(string type, JsonElement jsonObject)
        {
            // Preserve forward compatibility: surface unknown message types instead of failing the stream.
            return new StreamingSpeechGenerationMessageUnsupported
            {
                OriginalType = type,
                // Clone so the element survives disposal of the JsonDocument in Read().
                RawData = jsonObject.Clone()
            };
        }
    }
}
