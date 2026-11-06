using SerenityStar.Errors.Constants;

namespace SerenityStar.Errors.Models
{
    /// <summary>
    /// The API's own request limit was exceeded (<see cref="SerenityErrorCodes.RateLimitExceeded"/>, HTTP 429).
    /// See <see cref="SerenityApiError.RetryAfter"/> for how long to wait before retrying.
    /// </summary>
    /// <remarks>
    /// This limit is enforced by the Serenity API itself. A rate limit from the AI provider arrives instead as a
    /// <see cref="VendorErrorCodes.VendorRateLimitError"/> attempt inside an <see cref="AgentExecutionFailedError"/>
    /// or <see cref="AIServiceExecutionFailedError"/>.
    /// </remarks>
    public sealed class RateLimitExceededError : SerenityApiError
    {
    }
}
