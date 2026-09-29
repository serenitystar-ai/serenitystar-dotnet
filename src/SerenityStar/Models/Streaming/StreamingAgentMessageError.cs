using SerenityStar.Errors;
using SerenityStar.Errors.Models;

namespace SerenityStar.Models.Streaming
{
    /// <summary>
    /// Represents an error that happened after the stream opened.
    /// </summary>
    /// <remarks>
    /// Errors before the stream opens are thrown as <see cref="SerenityApiException"/>, carrying the same
    /// <see cref="SerenityApiError"/> types.
    /// </remarks>
    public sealed class StreamingAgentMessageError : StreamingAgentMessage
    {
        /// <inheritdoc />
        public override string Type => "error";

        /// <summary>
        /// The error. Pattern-match on its type, or branch on <see cref="SerenityApiError.Code"/>.
        /// </summary>
        public SerenityApiError Error { get; set; } = new SerenityApiError();

        /// <summary>
        /// Shortcut for <c>Error.Message</c>.
        /// </summary>
        public string Message => Error.Message;
    }
}
