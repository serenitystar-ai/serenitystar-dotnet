using SerenityStar.Constants;
using SerenityStar.Exceptions;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SerenityStar.Extensions
{
    /// <summary>
    /// Internal helpers for handling Serenity API HTTP responses consistently across scopes.
    /// </summary>
    internal static class HttpResponseMessageExtensions
    {
        /// <summary>
        /// Throws a <see cref="SerenityApiException"/> containing the status code, reason phrase and
        /// parsed JSON error payload when the response does not indicate success.
        /// </summary>
        internal static async Task EnsureSerenitySuccessAsync(this HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return;

            throw await response.ToSerenityApiExceptionAsync("Request failed");
        }

        /// <summary>
        /// Builds a <see cref="SerenityApiException"/> from an unsuccessful response, reading the body
        /// and parsing it as JSON when possible (the Serenity Star API returns JSON for all responses
        /// except HTTP 429).
        /// </summary>
        internal static async Task<SerenityApiException> ToSerenityApiExceptionAsync(
            this HttpResponseMessage response,
            string messagePrefix)
        {
            string errorContent = await response.Content.ReadAsStringAsync();

            return new SerenityApiException(
                $"{messagePrefix} with status code {response.StatusCode}",
                response.StatusCode,
                response.ReasonPhrase,
                TryParseJson(response.Content.Headers.ContentType?.MediaType, errorContent));
        }

        private static JsonElement? TryParseJson(string? mediaType, string content)
        {
            // The Serenity Star API returns JSON error bodies for every response except HTTP 429, which
            // has no JSON content type. Skip parsing when the content type is not JSON (e.g. text/plain).
            if (!IsJsonMediaType(mediaType) || string.IsNullOrWhiteSpace(content))
                return null;

            try
            {
                // Clone so the returned element is self-contained and the pooled document buffers
                // can be released, since the value is stored on a long-lived exception.
                using JsonDocument document = JsonDocument.Parse(content);
                return document.RootElement.Clone();
            }
            catch (JsonException)
            {
                // Defensive: the content type claimed JSON but the body was not valid JSON.
                return null;
            }
        }

        // Matches "application/json", "text/json" and structured suffixes such as
        // "application/problem+json", ignoring any charset parameter.
        private static bool IsJsonMediaType(string? mediaType) =>
            mediaType != null
            && (mediaType.EndsWith("/json", StringComparison.OrdinalIgnoreCase)
                || mediaType.EndsWith("+json", StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Ensures the response indicates success and deserializes its JSON body into
        /// <typeparamref name="T"/> using the SDK's camelCase serializer options.
        /// </summary>
        internal static async Task<T> ReadSerenityJsonAsync<T>(
            this HttpResponseMessage response,
            CancellationToken cancellationToken = default)
        {
            await response.EnsureSerenitySuccessAsync();

            return await response.Content.ReadFromJsonAsync<T>(JsonSerializerOptionsCache.s_camelCase, cancellationToken)
                   ?? throw new InvalidOperationException("Failed to deserialize response");
        }
    }
}
