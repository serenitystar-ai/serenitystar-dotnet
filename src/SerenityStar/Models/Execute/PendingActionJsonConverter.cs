using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SerenityStar.Models.Execute
{
    /// <summary>
    /// JSON converter for PendingAction that handles polymorphic deserialization based on the <c>type</c> property.
    /// </summary>
    /// <remarks>
    /// The concrete types are deserialized with the caller's options, so it works both for the camelCase REST
    /// responses and the snake_case streaming events.
    /// </remarks>
    internal class PendingActionJsonConverter : JsonConverter<PendingAction>
    {
        public override PendingAction Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using JsonDocument document = JsonDocument.ParseValue(ref reader);
            JsonElement jsonObject = document.RootElement;

            string? type = jsonObject.ValueKind == JsonValueKind.Object
                && jsonObject.TryGetProperty("type", out JsonElement typeElement)
                && typeElement.ValueKind == JsonValueKind.String
                    ? typeElement.GetString()
                    : null;

            PendingAction? pendingAction = type switch
            {
                "connection" => JsonSerializer.Deserialize<ConnectionPendingAction>(jsonObject, options),
                "approval" => JsonSerializer.Deserialize<ApprovalPendingAction>(jsonObject, options),
                "user_choice" => JsonSerializer.Deserialize<UserChoicePendingAction>(jsonObject, options),
                _ => CreateUnsupportedPendingAction(type, jsonObject)
            };

            return pendingAction ?? throw new JsonException($"Failed to deserialize pending action of type '{type}'");
        }

        public override void Write(Utf8JsonWriter writer, PendingAction value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }

        private static PendingAction CreateUnsupportedPendingAction(string? type, JsonElement jsonObject)
        {
            // Preserve forward compatibility: surface unknown pending actions instead of failing the whole response.
            return new UnsupportedPendingAction
            {
                OriginalType = type ?? string.Empty,
                // Clone so the element survives disposal of the JsonDocument in Read().
                RawData = jsonObject.Clone()
            };
        }
    }
}
