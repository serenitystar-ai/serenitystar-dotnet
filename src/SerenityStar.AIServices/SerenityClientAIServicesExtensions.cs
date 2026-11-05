using SerenityStar.AIServices;
using System;

namespace SerenityStar.Client
{
    /// <summary>
    /// Provides access to the Serenity Star AI services from a <see cref="SerenityClient"/>.
    /// Each method returns a lightweight client that uses the <see cref="SerenityClient"/> connection.
    /// </summary>
    public static class SerenityClientAIServicesExtensions
    {
        /// <summary>
        /// Gets a client to detect and obfuscate sensitive data (PII) in texts.
        /// </summary>
        /// <param name="client">The Serenity Star client.</param>
        /// <returns>An <see cref="IAnonymizerClient"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when client is null.</exception>
        public static IAnonymizerClient GetAnonymizerClient(this SerenityClient client)
            => new AnonymizerClient(GetApiClient(client));

        /// <summary>
        /// Gets a client to evaluate inputs against predicate, choice and score questions.
        /// </summary>
        /// <param name="client">The Serenity Star client.</param>
        /// <returns>An <see cref="IDecisionClient"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when client is null.</exception>
        public static IDecisionClient GetDecisionClient(this SerenityClient client)
            => new DecisionClient(GetApiClient(client));

        /// <summary>
        /// Gets a client to generate vector embeddings for texts.
        /// </summary>
        /// <param name="client">The Serenity Star client.</param>
        /// <returns>An <see cref="IEmbeddingClient"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when client is null.</exception>
        public static IEmbeddingClient GetEmbeddingClient(this SerenityClient client)
            => new EmbeddingClient(GetApiClient(client));

        /// <summary>
        /// Gets a client to generate images from text prompts.
        /// </summary>
        /// <param name="client">The Serenity Star client.</param>
        /// <returns>An <see cref="IImageGenerationClient"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when client is null.</exception>
        public static IImageGenerationClient GetImageGenerationClient(this SerenityClient client)
            => new ImageGenerationClient(GetApiClient(client));

        /// <summary>
        /// Gets a client to extract text, tables and images from documents (OCR).
        /// </summary>
        /// <param name="client">The Serenity Star client.</param>
        /// <returns>An <see cref="IOcrClient"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when client is null.</exception>
        public static IOcrClient GetOcrClient(this SerenityClient client)
            => new OcrClient(GetApiClient(client));

        /// <summary>
        /// Gets a client to generate speech (text-to-speech) from text.
        /// </summary>
        /// <param name="client">The Serenity Star client.</param>
        /// <returns>An <see cref="ISpeechGenerationClient"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when client is null.</exception>
        public static ISpeechGenerationClient GetSpeechGenerationClient(this SerenityClient client)
            => new SpeechGenerationClient(GetApiClient(client));

        /// <summary>
        /// Gets a client to transcribe audio and video files into text.
        /// </summary>
        /// <param name="client">The Serenity Star client.</param>
        /// <returns>An <see cref="ITranscriptionClient"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when client is null.</exception>
        public static ITranscriptionClient GetTranscriptionClient(this SerenityClient client)
            => new TranscriptionClient(GetApiClient(client));

        /// <summary>
        /// Gets a client to analyze images with vision models.
        /// </summary>
        /// <param name="client">The Serenity Star client.</param>
        /// <returns>An <see cref="IVisionClient"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when client is null.</exception>
        public static IVisionClient GetVisionClient(this SerenityClient client)
            => new VisionClient(GetApiClient(client));

        private static SerenityApiClient GetApiClient(SerenityClient client)
        {
            if (client is null)
                throw new ArgumentNullException(nameof(client));

            return client.ApiClient;
        }
    }
}
