using SerenityStar.Models.AIServices;
using System;
using System.Text.Json;

namespace SerenityStar.Models.SpeechGeneration
{
    /// <summary>
    /// Base class for all messages received while streaming speech generation.
    /// </summary>
    public abstract class StreamingSpeechGenerationMessage
    {
        /// <summary>
        /// The type of the streaming message.
        /// </summary>
        public abstract string Type { get; }
    }

    /// <summary>
    /// Opens the stream, describing the audio about to be received.
    /// </summary>
    public sealed class StreamingSpeechGenerationMessageStart : StreamingSpeechGenerationMessage
    {
        /// <inheritdoc />
        public override string Type => "start";

        /// <summary>
        /// The unique identifier of the speech generation instance.
        /// </summary>
        public Guid InstanceId { get; set; }

        /// <summary>
        /// The model used to generate the audio.
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// The vendor of the model.
        /// </summary>
        public string? Vendor { get; set; }

        /// <summary>
        /// The voice used to generate the audio.
        /// </summary>
        public string? Voice { get; set; }

        /// <summary>
        /// The MIME type of the audio chunks (e.g. "audio/mpeg"). It varies per vendor.
        /// </summary>
        public string? MimeType { get; set; }

        /// <summary>
        /// When the generation started (UTC).
        /// </summary>
        public DateTime StartTimeUtc { get; set; }
    }

    /// <summary>
    /// A chunk of generated audio. Concatenating every chunk in order yields the complete file.
    /// </summary>
    public sealed class StreamingSpeechGenerationMessageAudio : StreamingSpeechGenerationMessage
    {
        /// <inheritdoc />
        public override string Type => "audio";

        /// <summary>
        /// The position of this chunk in the stream.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The audio chunk, base64 encoded. Use <see cref="GetAudioBytes"/> to decode it.
        /// </summary>
        public string Data { get; set; } = string.Empty;

        /// <summary>
        /// Decodes <see cref="Data"/> into the raw audio bytes.
        /// </summary>
        /// <returns>The raw audio bytes of this chunk.</returns>
        public byte[] GetAudioBytes() => Convert.FromBase64String(Data);
    }

    /// <summary>
    /// Represents a keep-alive ping during streaming.
    /// </summary>
    public sealed class StreamingSpeechGenerationMessagePing : StreamingSpeechGenerationMessage
    {
        /// <inheritdoc />
        public override string Type => "ping";
    }

    /// <summary>
    /// Represents an error during streaming. Carries the same fields as the non-streaming error response.
    /// </summary>
    public sealed class StreamingSpeechGenerationMessageError : StreamingSpeechGenerationMessage
    {
        /// <inheritdoc />
        public override string Type => "error";

        /// <summary>
        /// The error code.
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// The error message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// A link to documentation about the error, if any.
        /// </summary>
        public string? DocumentationUrl { get; set; }

        /// <summary>
        /// Additional error details, if any, as raw JSON.
        /// </summary>
        public JsonElement? Errors { get; set; }

        /// <summary>
        /// The errors of each model attempt (e.g. fallbacks), if any, as raw JSON.
        /// </summary>
        public JsonElement? Attempts { get; set; }

        /// <summary>
        /// When rate limited, the number of seconds to wait before retrying.
        /// </summary>
        public int? RetryAfterSeconds { get; set; }
    }

    /// <summary>
    /// Closes the stream with the same payload the non-streaming generation returns.
    /// </summary>
    public sealed class StreamingSpeechGenerationMessageStop : StreamingSpeechGenerationMessage
    {
        /// <inheritdoc />
        public override string Type => "stop";

        /// <summary>
        /// The unique identifier of the speech generation instance.
        /// </summary>
        public Guid InstanceId { get; set; }

        /// <summary>
        /// The public URL to download the complete generated audio.
        /// </summary>
        public string? AudioUrl { get; set; }

        /// <summary>
        /// The billed quantities in the units the model is priced by.
        /// </summary>
        public AIServiceUsage? Usage { get; set; }

        /// <summary>
        /// The cost information for this execution in the tenant's currency.
        /// </summary>
        public AIServiceCost? Cost { get; set; }

        /// <summary>
        /// When the generation finished (UTC).
        /// </summary>
        public DateTime StopTimeUtc { get; set; }
    }

    /// <summary>
    /// Represents a streaming message whose <c>type</c> is not recognized by this SDK version.
    /// This is emitted (instead of an error) when the API introduces a new message type, so that
    /// consumers can safely ignore or inspect it without the stream failing.
    /// </summary>
    public sealed class StreamingSpeechGenerationMessageUnsupported : StreamingSpeechGenerationMessage
    {
        /// <inheritdoc />
        public override string Type => "unsupported";

        /// <summary>
        /// The original, unrecognized <c>type</c> value reported by the API.
        /// </summary>
        public string OriginalType { get; set; } = string.Empty;

        /// <summary>
        /// The raw JSON payload of the message, so consumers can inspect it if needed.
        /// This is a detached copy, safe to read after deserialization completes.
        /// </summary>
        public JsonElement? RawData { get; set; }
    }
}
