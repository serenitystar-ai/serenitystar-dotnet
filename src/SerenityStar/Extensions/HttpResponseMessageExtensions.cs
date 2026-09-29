using SerenityStar.Constants;
using SerenityStar.Errors;
using SerenityStar.Errors.Constants;
using SerenityStar.Errors.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
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
        /// Throws a <see cref="SerenityApiException"/> when the response is not successful.
        /// </summary>
        internal static async Task EnsureSerenitySuccessAsync(this HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return;

            string content = await response.Content.ReadAsStringAsync();

            SerenityApiError error = TryParse(content) ?? new SerenityApiError { Code = SerenityErrorCodes.Unknown };

            // Keep the raw body only when it couldn't be read as a Serenity API error, as it's the only diagnostic left.
            string? responseContent = error.Code == SerenityErrorCodes.Unknown ? content : null;

            if (string.IsNullOrEmpty(error.Message))
                error.Message = string.IsNullOrEmpty(response.ReasonPhrase)
                    ? $"HTTP {(int)response.StatusCode}"
                    : response.ReasonPhrase;

            error.RetryAfter = ReadRetryAfter(response.Headers.RetryAfter) ?? error.RetryAfter;

            throw new SerenityApiException(response.StatusCode, error, responseContent);
        }

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

        private static SerenityApiError? TryParse(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return null;

            try
            {
                using JsonDocument document = JsonDocument.Parse(content);
                return SerenityApiErrorParser.Parse(document.RootElement, JsonSerializerOptionsCache.s_camelCase);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static TimeSpan? ReadRetryAfter(RetryConditionHeaderValue? retryAfter)
        {
            if (retryAfter?.Delta is TimeSpan delta)
                return delta;

            if (retryAfter?.Date is DateTimeOffset date)
            {
                TimeSpan wait = date - DateTimeOffset.UtcNow;
                return wait > TimeSpan.Zero ? wait : TimeSpan.Zero;
            }

            return null;
        }
    }
}
