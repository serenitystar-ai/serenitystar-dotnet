using SerenityStar.Errors.Constants;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SerenityStar.Errors.Models
{
    /// <summary>
    /// One attempt to call the AI provider, inside an <see cref="AgentExecutionFailedError"/> or
    /// <see cref="AIServiceExecutionFailedError"/>.
    /// </summary>
    public class ExecutionAttempt
    {
        /// <summary>
        /// The 0-based position of this attempt.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The code this failure would have on its own, usually one of <see cref="VendorErrorCodes"/>.
        /// Null only when that error has no code.
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// The raw HTTP status the AI provider returned (e.g. 429, 503, 529). Timeouts and cancellations carry 504.
        /// Null when the provider returned no status. It is informational and doesn't drive the response status.
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// Human-readable summary of this attempt's failure.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Details of this attempt's failure. Vendor faults carry the provider's detail under
        /// <see cref="VendorErrorCodes.VendorError"/>.
        /// </summary>
        public Dictionary<string, string>? Errors { get; set; }

        /// <summary>
        /// The provider's own error detail, already redacted by the API. Null when the attempt has none.
        /// </summary>
        [JsonIgnore]
        public string? VendorError =>
            Errors != null && Errors.TryGetValue(VendorErrorCodes.VendorError, out string? detail) ? detail : null;
    }
}
