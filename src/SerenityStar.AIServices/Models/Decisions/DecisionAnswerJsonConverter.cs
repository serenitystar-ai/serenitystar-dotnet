using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SerenityStar.Models.Decisions
{
    /// <summary>
    /// JSON converter for DecisionAnswer that handles polymorphic deserialization.
    /// </summary>
    internal class DecisionAnswerJsonConverter : JsonConverter<DecisionAnswer>
    {
        /// <summary>
        /// Options with CamelCase naming policy and this converter, used to read decision results.
        /// </summary>
        /// <remarks>
        /// Cached here instead of JsonSerializerOptionsCache because SerenityStar.Core must not know AI service types.
        /// Read why caching is needed: "CA1869: Cache and reuse 'JsonSerializerOptions' instances"
        /// https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1869
        /// </remarks>
        internal static readonly JsonSerializerOptions s_options = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new DecisionAnswerJsonConverter() }
        };

        public override DecisionAnswer Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            using JsonDocument document = JsonDocument.ParseValue(ref reader);
            JsonElement jsonObject = document.RootElement;

            if (!jsonObject.TryGetProperty("type", out JsonElement typeElement))
            {
                throw new JsonException("Missing 'type' property in decision answer");
            }

            string? type = typeElement.ValueKind == JsonValueKind.String ? typeElement.GetString() : null;

            if (type == null)
            {
                throw new JsonException("'type' property must be a non-null string");
            }

            DecisionAnswer answer = type switch
            {
                "predicate" => JsonSerializer.Deserialize<PredicateDecisionAnswer>(jsonObject, options),
                "choice" => JsonSerializer.Deserialize<ChoiceDecisionAnswer>(jsonObject, options),
                "score" => JsonSerializer.Deserialize<ScoreDecisionAnswer>(jsonObject, options),
                "refusal" => JsonSerializer.Deserialize<RefusalDecisionAnswer>(jsonObject, options),
                _ => CreateUnsupportedAnswer(type, jsonObject, options)
            } ?? throw new JsonException($"Failed to deserialize decision answer of type '{type}'");

            return answer;
        }

        public override void Write(Utf8JsonWriter writer, DecisionAnswer value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
        }

        private static DecisionAnswer CreateUnsupportedAnswer(string type, JsonElement jsonObject, JsonSerializerOptions options)
        {
            // Preserve forward compatibility: surface unknown answer types instead of failing the whole result.
            UnsupportedDecisionAnswer answer = JsonSerializer.Deserialize<UnsupportedDecisionAnswer>(jsonObject, options)
                ?? new UnsupportedDecisionAnswer();
            answer.OriginalType = type;
            // Clone so the element survives disposal of the JsonDocument in Read().
            answer.RawData = jsonObject.Clone();
            return answer;
        }
    }
}
