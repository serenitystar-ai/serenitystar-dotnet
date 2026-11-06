using SerenityStar.Errors.Models;

namespace SerenityStar.Errors.Constants
{
    /// <summary>
    /// The values of <see cref="SerenityApiError.Code"/>.
    /// </summary>
    public static class SerenityErrorCodes
    {
        /// <summary>
        /// HTTP 401. Authentication is missing or invalid.
        /// </summary>
        public const string Unauthorized = "unauthorized";

        /// <summary>
        /// HTTP 403. Authenticated, but without permission to access the resource.
        /// </summary>
        public const string Forbidden = "forbidden";

        /// <summary>
        /// HTTP 400. The request itself failed validation. See <see cref="Models.InputValidationError"/>.
        /// </summary>
        public const string InputValidationError = "input_validation_error";

        /// <summary>
        /// HTTP 400. A business rule rejected the request. See <see cref="Models.ValidationError"/>.
        /// </summary>
        public const string ValidationError = "validation_error";

        /// <summary>
        /// An agent's call to the AI model failed. See <see cref="AgentExecutionFailedError"/>.
        /// </summary>
        public const string AgentExecutionFailed = "agent_execution_failed";

        /// <summary>
        /// An AI service request failed at the AI provider. See <see cref="AIServiceExecutionFailedError"/>.
        /// </summary>
        public const string AIServiceExecutionFailed = "aiservice_execution_failed";

        /// <summary>
        /// HTTP 413. The request body exceeds the maximum size.
        /// </summary>
        public const string RequestTooLarge = "request_too_large";

        /// <summary>
        /// HTTP 405. The route doesn't support the HTTP method used.
        /// </summary>
        public const string MethodNotAllowed = "method_not_allowed";

        /// <summary>
        /// HTTP 415. The request <c>Content-Type</c> isn't supported by the endpoint.
        /// </summary>
        public const string UnsupportedMediaType = "unsupported_media_type";

        /// <summary>
        /// HTTP 404. The requested resource does not exist. See <see cref="ResourceNotFoundError"/>.
        /// </summary>
        public const string ResourceNotFound = "resource_not_found";

        /// <summary>
        /// HTTP 429. The API's own request limit was exceeded. See <see cref="RateLimitExceededError"/>.
        /// </summary>
        public const string RateLimitExceeded = "rate_limit_exceeded";

        /// <summary>
        /// HTTP 500. An unexpected server-side failure.
        /// </summary>
        public const string ServerError = "server_error";

        /// <summary>
        /// Set by the SDK, never sent by the API: the response wasn't a Serenity API error, for example an HTML
        /// page from a proxy or an empty body. <see cref="SerenityApiException.ResponseContent"/> holds the raw body.
        /// </summary>
        public const string Unknown = "unknown";
    }
}
