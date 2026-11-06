using System.Text.Json;

namespace SerenityStar.Models.Execute
{
    /// <summary>
    /// Represents a pending action whose <c>type</c> is not recognized by this SDK version.
    /// This is returned (instead of failing the deserialization) when the API introduces a new kind of pending action,
    /// so that consumers can safely ignore or inspect it.
    /// </summary>
    public sealed class UnsupportedPendingAction : PendingAction
    {
        /// <inheritdoc />
        public override string Type => "unsupported";

        /// <summary>
        /// The original, unrecognized <c>type</c> value reported by the API.
        /// </summary>
        public string OriginalType { get; set; } = string.Empty;

        /// <summary>
        /// The raw JSON payload of the pending action, so consumers can inspect it if needed.
        /// This is a detached copy, safe to read after deserialization completes.
        /// </summary>
        public JsonElement? RawData { get; set; }
    }
}
