using SerenityStar.Constants;
using System;
using System.Net.Http;
using System.Net.Http.Json;
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
        /// Throws an <see cref="HttpRequestException"/> containing the response body when the response
        /// does not indicate success.
        /// </summary>
        internal static async Task EnsureSerenitySuccessAsync(this HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
                return;

            string errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
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
    }
}
