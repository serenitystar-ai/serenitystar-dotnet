using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SerenityStar.Models.Streaming
{
    /// <summary>
    /// JSON converter for StreamingAgentMessage that handles polymorphic deserialization.
    /// </summary>
    internal class StreamingAgentMessageJsonConverter : JsonConverter<StreamingAgentMessage>
    {
        public override StreamingAgentMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using JsonDocument document = JsonDocument.ParseValue(ref reader);
            JsonElement jsonObject = document.RootElement;

            if (!jsonObject.TryGetProperty("type", out JsonElement typeElement))
            {
                throw new JsonException("Missing 'type' property in streaming message");
            }

            string? type = typeElement.GetString();

            if (type == null)
            {
                throw new JsonException("'type' property cannot be null");
            }

            StreamingAgentMessage message = type switch
            {
                "task_start" => JsonSerializer.Deserialize<StreamingAgentMessageTaskStart>(jsonObject, options),
                "content" => JsonSerializer.Deserialize<StreamingAgentMessageContent>(jsonObject, options),
                "task_end" => JsonSerializer.Deserialize<StreamingAgentMessageTaskEnd>(jsonObject, options),
                "stop" => JsonSerializer.Deserialize<StreamingAgentMessageStop>(jsonObject, options),
                "error" => JsonSerializer.Deserialize<StreamingAgentMessageError>(jsonObject, options),
                _ => CreateUnsupportedMessage(jsonObject, type)
            } ?? throw new JsonException($"Failed to deserialize streaming message of type '{type}'");

            return message;
        }

        public override void Write(Utf8JsonWriter writer, StreamingAgentMessage value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }

        private static StreamingAgentMessage CreateUnsupportedMessage(JsonElement jsonObject, string type)
        {
            // Return a generic message for unsupported types
            // Could create a specific UnsupportedStreamingMessage class if needed
            return new StreamingAgentMessageError
            {
                Type = type,
                Error = $"Unsupported message type: {type}"
            };
        }
    }
}
