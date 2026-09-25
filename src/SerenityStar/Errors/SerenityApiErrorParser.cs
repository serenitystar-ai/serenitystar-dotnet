using SerenityStar.Errors.Constants;
using SerenityStar.Errors.Models;
using System;
using System.Text.Json;

namespace SerenityStar.Errors
{
    /// <summary>
    /// Reads a Serenity API error payload into the <see cref="SerenityApiError"/> type that matches its code.
    /// </summary>
    /// <remarks>
    /// The same payload arrives camelCase over HTTP and snake_case over SSE; the casing comes from the options passed in.
    /// This is a helper rather than a registered <c>JsonConverter</c>, because <c>[JsonPolymorphic]</c> can't handle it:
    /// it requires the discriminator to be the first property, throws on unrecognised codes and can't also bind
    /// the discriminator to <see cref="SerenityApiError.Code"/>.
    /// </remarks>
    internal static class SerenityApiErrorParser
    {
        private const string RetryAfterSecondsProperty = "RetryAfterSeconds";

        /// <summary>
        /// Parses an error payload. Never throws: a payload that doesn't match its documented shape falls back to
        /// the base type, and one that isn't an error at all reads <see cref="SerenityErrorCodes.Unknown"/>.
        /// </summary>
        internal static SerenityApiError Parse(JsonElement json, JsonSerializerOptions options)
        {
            if (json.ValueKind != JsonValueKind.Object)
                return new SerenityApiError { Code = SerenityErrorCodes.Unknown };

            string? code = json.TryGetProperty("code", out JsonElement codeElement) && codeElement.ValueKind == JsonValueKind.String
                ? codeElement.GetString()
                : null;

            SerenityApiError error = DeserializeTyped(json, code, options)
                ?? DeserializeBase(json, options)
                ?? new SerenityApiError();

            if (string.IsNullOrEmpty(error.Code))
                error.Code = string.IsNullOrEmpty(code) ? SerenityErrorCodes.Unknown : code!;

            ReadRetryAfterSeconds(error, options);

            return error;
        }

        /// <summary>
        /// Removes a property from <see cref="SerenityApiError.ExtensionData"/>, for properties the caller models itself.
        /// </summary>
        internal static void RemoveExtensionData(SerenityApiError error, string propertyName)
        {
            if (error.ExtensionData == null)
                return;

            error.ExtensionData.Remove(propertyName);

            if (error.ExtensionData.Count == 0)
                error.ExtensionData = null;
        }

        private static SerenityApiError? DeserializeTyped(JsonElement json, string? code, JsonSerializerOptions options)
        {
            try
            {
                return code switch
                {
                    SerenityErrorCodes.InputValidationError => JsonSerializer.Deserialize<InputValidationError>(json, options),
                    SerenityErrorCodes.ValidationError => JsonSerializer.Deserialize<ValidationError>(json, options),
                    SerenityErrorCodes.ResourceNotFound => JsonSerializer.Deserialize<ResourceNotFoundError>(json, options),
                    SerenityErrorCodes.RateLimitExceeded => JsonSerializer.Deserialize<RateLimitExceededError>(json, options),
                    SerenityErrorCodes.AgentExecutionFailed => JsonSerializer.Deserialize<AgentExecutionFailedError>(json, options),
                    SerenityErrorCodes.AIServiceExecutionFailed => JsonSerializer.Deserialize<AIServiceExecutionFailedError>(json, options),
                    _ => JsonSerializer.Deserialize<SerenityApiError>(json, options)
                };
            }
            catch (Exception ex) when (ex is JsonException || ex is NotSupportedException)
            {
                // The payload doesn't match the documented shape for its code: keep what the base type can read.
                return null;
            }
        }

        private static SerenityApiError? DeserializeBase(JsonElement json, JsonSerializerOptions options)
        {
            try
            {
                return JsonSerializer.Deserialize<SerenityApiError>(json, options);
            }
            catch (Exception ex) when (ex is JsonException || ex is NotSupportedException)
            {
                return null;
            }
        }

        private static void ReadRetryAfterSeconds(SerenityApiError error, JsonSerializerOptions options)
        {
            string propertyName = options.PropertyNamingPolicy?.ConvertName(RetryAfterSecondsProperty) ?? RetryAfterSecondsProperty;

            if (error.ExtensionData == null || !error.ExtensionData.TryGetValue(propertyName, out JsonElement value))
                return;

            if (value.ValueKind != JsonValueKind.Number || !value.TryGetDouble(out double seconds) || seconds < 0)
                return;

            error.RetryAfter = TimeSpan.FromSeconds(seconds);
            RemoveExtensionData(error, propertyName);
        }
    }
}
