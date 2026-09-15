using SerenityStar.Constants;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SerenityStar.Client
{
    /// <summary>
    /// Internal wrapper around <see cref="HttpClient"/> that attaches Serenity authentication and
    /// resolves relative request URIs against the configured base URL.
    /// </summary>
    /// <remarks>
    /// Authentication is applied per <see cref="HttpRequestMessage"/> rather than through
    /// <see cref="HttpClient.DefaultRequestHeaders"/>, so a caller-provided <see cref="HttpClient"/>
    /// is never mutated and there is no risk of a duplicate-header exception when the client is reused.
    /// </remarks>
    internal sealed class SerenityApiClient
    {
        private const string ApiKeyHeaderName = "X-API-KEY";

        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly Uri _baseAddress;

        internal SerenityApiClient(HttpClient httpClient, string apiKey, string? baseUrl = null)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _baseAddress = new Uri(baseUrl ?? ClientConstants.BaseUrl);
        }

        /// <summary>
        /// Sends the request, buffering the full response before returning.
        /// </summary>
        internal Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken = default)
            => SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);

        /// <summary>
        /// Sends the request using the specified completion option. Use
        /// <see cref="HttpCompletionOption.ResponseHeadersRead"/> for streaming responses.
        /// </summary>
        internal Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            HttpCompletionOption completionOption,
            CancellationToken cancellationToken = default)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            // Resolve relative URIs ourselves so we neither rely on nor mutate HttpClient.BaseAddress.
            if (request.RequestUri is null)
                request.RequestUri = _baseAddress;
            else if (!request.RequestUri.IsAbsoluteUri)
                request.RequestUri = new Uri(_baseAddress, request.RequestUri);

            // Attach auth per request. Skip if the caller already set the header on the client so we
            // never send it twice.
            if (!request.Headers.Contains(ApiKeyHeaderName))
                request.Headers.Add(ApiKeyHeaderName, _apiKey);

            return _httpClient.SendAsync(request, completionOption, cancellationToken);
        }
    }
}
