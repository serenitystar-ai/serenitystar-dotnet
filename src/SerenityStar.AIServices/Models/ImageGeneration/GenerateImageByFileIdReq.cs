using System;
using System.Collections.Generic;
using System.Linq;

namespace SerenityStar.Models.ImageGeneration
{
    /// <summary>
    /// Request for generating an image from a text prompt and source images previously uploaded to Serenity Star.
    /// </summary>
    public sealed class GenerateImageByFileIdReq
    {
        /// <summary>
        /// The AI Model Id to use for image generation.
        /// If not provided, the default image model will be used.
        /// </summary>
        public Guid? ModelId { get; set; }

        /// <summary>
        /// The ids of the uploaded source images. At least one is required.
        /// Supported formats: jpg, jpeg, png, gif, webp. Maximum size per file: 20 MB.
        /// </summary>
        public List<Guid> FileIds { get; set; } = new List<Guid>();

        /// <summary>
        /// The description of the image to generate. Maximum length: 1000 characters.
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
        /// <exception cref="ArgumentNullException">Thrown when Prompt is null or empty.</exception>
        /// <exception cref="ArgumentException">Thrown when no file ids are provided or any is empty.</exception>
        public void Validate()
        {
            if (string.IsNullOrEmpty(Prompt))
                throw new ArgumentNullException(nameof(Prompt), "Prompt is required for image generation.");
            if (FileIds is null || FileIds.Count == 0 || FileIds.Any(id => id == Guid.Empty))
                throw new ArgumentException("At least one non-empty file id is required.", nameof(FileIds));
        }
    }
}
