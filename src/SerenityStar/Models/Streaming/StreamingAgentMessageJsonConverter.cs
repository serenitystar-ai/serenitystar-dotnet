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
                "start" => JsonSerializer.Deserialize<StreamingAgentMessageStart>(jsonObject, options),
                "task_start" => JsonSerializer.Deserialize<StreamingAgentMessageTaskStart>(jsonObject, options),
                "content" => JsonSerializer.Deserialize<StreamingAgentMessageContent>(jsonObject, options),
                "reasoning" => JsonSerializer.Deserialize<StreamingAgentMessageReasoning>(jsonObject, options),
                "task_stop" => JsonSerializer.Deserialize<StreamingAgentMessageTaskStop>(jsonObject, options),
                "stop" => JsonSerializer.Deserialize<StreamingAgentMessageStop>(jsonObject, options),
                "error" => JsonSerializer.Deserialize<StreamingAgentMessageError>(jsonObject, options),
                "ping" => JsonSerializer.Deserialize<StreamingAgentMessagePing>(jsonObject, options),
                _ => CreateUnsupportedMessage(type, jsonObject)
            } ?? throw new JsonException($"Failed to deserialize streaming message of type '{type}'");

            return message;
        }

        public override void Write(Utf8JsonWriter writer, StreamingAgentMessage value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }

        private static StreamingAgentMessage CreateUnsupportedMessage(string type, JsonElement jsonObject)
        {
            // Preserve forward compatibility: surface unknown message types instead of failing the stream.
            return new StreamingAgentMessageUnsupported
            {
                OriginalType = type,
                // Clone so the element survives disposal of the JsonDocument in Read().
                RawData = jsonObject.Clone()
            };
        }
    }
}
