using SerenityStar.Errors.Constants;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SerenityStar.Errors.Models
{
    /// <summary>
    /// An error returned by the Serenity API, either as an HTTP error response or as a streaming error event.
    /// </summary>
    /// <remarks>
    /// Codes that carry extra data are represented by a subclass, such as <see cref="ValidationError"/> or
    /// <see cref="AgentExecutionFailedError"/>. This base type is used as-is for codes that carry no extra data,
    /// for codes this SDK version doesn't recognise, and for responses that aren't Serenity API errors at all
    /// (<see cref="SerenityErrorCodes.Unknown"/>).
    /// </remarks>
    public class SerenityApiError
    {
        /// <summary>
        /// Stable, machine-readable error code. Branch on this, not on <see cref="Message"/>.
        /// See <see cref="SerenityErrorCodes"/>.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable summary of the error. It is localised, so don't depend on its wording.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Deep link to this error's section of the API documentation. Null when the API didn't send one.
        /// </summary>
        public string? DocumentationUrl { get; set; }

        /// <summary>
        /// How long the API asked you to wait before retrying, if it said so.
        /// Comes from the <c>Retry-After</c> header on HTTP responses, or from <c>retry_after_seconds</c> on streaming error events.
        /// </summary>
        /// <remarks>
        /// The API sends it with <see cref="SerenityErrorCodes.RateLimitExceeded"/>, and with
        /// <see cref="SerenityErrorCodes.AgentExecutionFailed"/> and <see cref="SerenityErrorCodes.AIServiceExecutionFailed"/>
        /// when every attempt was rate limited by the AI provider.
        /// </remarks>
        [JsonIgnore]
        public TimeSpan? RetryAfter { get; set; }

        /// <summary>
        /// Fields this SDK version doesn't model, such as the <c>errors</c> map of an unrecognised code.
        /// Null when there are none.
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    }
}
