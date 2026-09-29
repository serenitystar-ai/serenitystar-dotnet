using SerenityStar.Errors.Constants;
using SerenityStar.Errors.Models;
using SerenityStar.Models.Streaming;
using System;
using System.Net;
using System.Net.Http;

namespace SerenityStar.Errors
{
    /// <summary>
    /// Thrown when the Serenity API returns a non-success HTTP response.
    /// </summary>
    /// <remarks>
    /// On streaming calls this is thrown only when the request fails before the stream opens. Errors that happen
    /// after the stream opens are delivered as <see cref="StreamingAgentMessageError"/> messages,
    /// carrying the same <see cref="SerenityApiError"/> types.
    /// </remarks>
    public sealed class SerenityApiException : HttpRequestException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerenityApiException"/> class.
        /// </summary>
        /// <param name="statusCode">The HTTP status of the response.</param>
        /// <param name="error">The error the API returned.</param>
        /// <param name="responseContent">The raw response body, when it couldn't be parsed as a Serenity API error.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="error"/> is null.</exception>
        public SerenityApiException(HttpStatusCode statusCode, SerenityApiError error, string? responseContent = null)
            : base((error ?? throw new ArgumentNullException(nameof(error))).Message)
        {
            StatusCode = statusCode;
            Error = error;
            ResponseContent = responseContent;
        }

        /// <summary>
        /// The HTTP status of the response.
        /// </summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>
        /// The error the API returned. Pattern-match on its type, or branch on <see cref="SerenityApiError.Code"/>.
        /// </summary>
        public SerenityApiError Error { get; }

        /// <summary>
        /// The raw response body. Only set when it couldn't be parsed as a Serenity API error
        /// (<see cref="SerenityErrorCodes.Unknown"/>).
        /// </summary>
        public string? ResponseContent { get; }
    }
}
