using System.Net;
using System.Net.Http;
using System.Text.Json;

namespace SerenityStar.Exceptions
{
    /// <summary>
    /// Thrown when the Serenity Star API returns an unsuccessful HTTP response. Derives from
    /// <see cref="HttpRequestException"/> so existing <c>catch (HttpRequestException)</c> handlers keep
    /// working, while exposing the response <see cref="StatusCode"/>, <see cref="ReasonPhrase"/> and the
    /// parsed <see cref="ResponseJson"/> error payload for inspection.
    /// </summary>
    public class SerenityApiException : HttpRequestException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerenityApiException"/> class.
        /// </summary>
        /// <param name="message">The error message describing the failure.</param>
        /// <param name="statusCode">The HTTP status code returned by the API.</param>
        /// <param name="reasonPhrase">The HTTP reason phrase returned by the API, if any.</param>
        /// <param name="responseJson">The parsed JSON error payload, or <c>null</c> when the body was not JSON (e.g. HTTP 429).</param>
        public SerenityApiException(
            string message,
            HttpStatusCode statusCode,
            string? reasonPhrase = null,
            JsonElement? responseJson = null)
            : base(message)
        {
            StatusCode = statusCode;
            ReasonPhrase = reasonPhrase;
            ResponseJson = responseJson;
        }

        /// <summary>
        /// The HTTP status code returned by the API.
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// The HTTP reason phrase returned by the API, if any.
        /// </summary>
        public string? ReasonPhrase { get; }

        /// <summary>
        /// The parsed JSON error payload returned by the API. The Serenity Star API returns JSON error
        /// bodies for all responses except HTTP 429 (rate limiting); this is <c>null</c> when the body
        /// was missing or not valid JSON. Use <see cref="JsonElement.GetRawText"/> to read the raw text.
        /// </summary>
        public JsonElement? ResponseJson { get; }
    }
}
