using SerenityStar.Errors.Models;

namespace SerenityStar.Errors.Constants
{
    /// <summary>
    /// The values of <see cref="ExecutionAttempt.Code"/> for failures of the upstream AI provider.
    /// These never appear as the top-level <see cref="SerenityApiError.Code"/>.
    /// </summary>
    public static class VendorErrorCodes
    {
        /// <summary>
        /// The provider rejected the request as client-fixable (HTTP 400, 413, 415 or 422).
        /// </summary>
        public const string VendorValidationError = "vendor_validation_error";

        /// <summary>
        /// The prompt exceeded the model's context window. Client-fixable, and never retried.
        /// </summary>
        public const string VendorContextLengthError = "vendor_context_length_error";

        /// <summary>
        /// The provider rate limited the request (HTTP 429).
        /// </summary>
        public const string VendorRateLimitError = "vendor_rate_limit_error";

        /// <summary>
        /// The provider rejected the configured credentials (HTTP 401 or 403).
        /// </summary>
        public const string VendorAuthenticationError = "vendor_authentication_error";

        /// <summary>
        /// The provider is unavailable or overloaded, or returned any other status (5xx, 404, 408, ...), or none.
        /// </summary>
        public const string VendorServiceError = "vendor_service_error";

        /// <summary>
        /// The request to the provider timed out before a response was received.
        /// </summary>
        public const string VendorTimeoutError = "vendor_timeout_error";

        /// <summary>
        /// The call to the provider was cancelled before it completed.
        /// </summary>
        public const string VendorCancellationError = "vendor_cancellation_error";

        /// <summary>
        /// The <see cref="ExecutionAttempt.Errors"/> key that carries the provider's own error detail.
        /// Also reserved as a vendor error code.
        /// </summary>
        public const string VendorError = "vendor_error";
    }
}
