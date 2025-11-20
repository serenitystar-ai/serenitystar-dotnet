using SerenityStar.Models.Streaming;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SerenityStar.Constants
{
    /// <summary>
    /// Static instance of JsonSerializerOptions used for serialization.
    /// </summary>
    /// <remarks>
    /// Read why this class is needed here: "CA1869: Cache and reuse 'JsonSerializerOptions' instances"
    /// https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1869
    /// </remarks>
    public class JsonSerializerOptionsCache
    {
        /// <summary>
        /// Options with CamelCase naming policy and case-insensitive property matching.
        /// </summary>
        public static readonly JsonSerializerOptions s_camelCase = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Options with SnakeCaseLower naming policy and case-insensitive property matching.
        /// </summary>
        public static readonly JsonSerializerOptions s_snakeCaseLower = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        /// <summary>
        /// Options with CamelCase naming policy and ignoring null values when writing.
        /// </summary>
        public static readonly JsonSerializerOptions s_camelCaseIgnoreNull = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Options with SnakeCaseLower naming policy for streaming messages with custom converter.
        /// </summary>
        public static readonly JsonSerializerOptions s_streamingSnakeCaseLower = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            Converters = { new StreamingAgentMessageJsonConverter() }
        };
    }
}
