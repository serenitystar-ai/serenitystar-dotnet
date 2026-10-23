using System;

namespace SerenityStar.Models.SpeechGeneration
{
    /// <summary>
    /// Request for generating speech (text-to-speech) from text.
    /// </summary>
    /// <remarks>
    /// A model is required: set <see cref="AIModelId"/>, or both <see cref="ModelCode"/> and <see cref="Vendor"/>.
    /// </remarks>
    public sealed class GenerateSpeechReq
    {
        /// <summary>
        /// The id of the speech generation AI Model to use. Do not combine with <see cref="ModelCode"/> and <see cref="Vendor"/>.
        /// </summary>
        public Guid? AIModelId { get; set; }

        /// <summary>
        /// The code of the speech generation model to use (e.g. "tts-1"). Must be combined with <see cref="Vendor"/>.
        /// </summary>
        public string? ModelCode { get; set; }

        /// <summary>
        /// The vendor of the model identified by <see cref="ModelCode"/> (e.g. "OpenAI", "ElevenLabs").
        /// </summary>
        public string? Vendor { get; set; }

        /// <summary>
        /// The text to convert to speech. Maximum length: 4000 characters.
        /// </summary>
        public string Text { get; set; } = null!;

        /// <summary>
        /// Optional voice name. If not provided, the vendor's default voice is used.
        /// </summary>
        public string? Voice { get; set; }

        /// <summary>
        /// Optional raw vendor voice id. Takes precedence over <see cref="Voice"/>. Maximum length: 128 characters.
        /// </summary>
        public string? VoiceId { get; set; }

        /// <summary>
        /// Optional speech speed. Defaults to 1, is clamped to the range the model supports
        /// and is ignored by models that don't accept a speed.
        /// </summary>
        public float? Speed { get; set; }

        /// <summary>
        /// Optional user identifier for tracking and cost management purposes.
        /// </summary>
        public string? UserIdentifier { get; set; }

        /// <summary>
        /// The channel used in the execution.
        /// </summary>
        public string? Channel { get; set; }

        /// <summary>
        /// Validates the request parameters.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when Text is null or empty.</exception>
        public void Validate()
        {
            if (string.IsNullOrEmpty(Text))
                throw new ArgumentNullException(nameof(Text), "Text is required for speech generation.");
        }
    }
}
