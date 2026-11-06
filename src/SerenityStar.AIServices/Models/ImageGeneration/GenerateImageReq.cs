using System;
using System.Text.Json.Serialization;

namespace SerenityStar.Models.ImageGeneration
{
    /// <summary>
    /// Request for generating an image from a text prompt.
    /// </summary>
    public sealed class GenerateImageReq
    {
        /// <summary>
        /// The AI Model Id to use for image generation.
        /// If not provided, the default image model will be used.
        /// </summary>
        [JsonPropertyName("modelId")]
        public Guid? AIModelId { get; set; }

        /// <summary>
        /// The description of the image to generate.
        /// </summary>
        public string Prompt { get; set; } = null!;

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
        /// <exception cref="ArgumentNullException">Thrown when Prompt is null, empty or whitespace.</exception>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Prompt))
                throw new ArgumentNullException(nameof(Prompt), "Prompt is required for image generation.");
        }
    }
}
