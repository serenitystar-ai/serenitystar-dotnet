using SerenityStar.Models.AIServices;
using System;
using System.Collections.Generic;

namespace SerenityStar.Models.ImageGeneration
{
    /// <summary>
    /// Request for generating an image from a text prompt and source images uploaded directly.
    /// </summary>
    public sealed class GenerateImageWithSourceImagesReq
    {
        /// <summary>
        /// The AI Model Id to use for image generation.
        /// If not provided, the default image model will be used.
        /// Only models that accept source images support it, each up to its own number of images.
        /// </summary>
        public Guid? AIModelId { get; set; }

        /// <summary>
        /// The source images, in the order the prompt refers to them. At least one is required.
        /// The source images are not stored.
        /// </summary>
        public List<AIServiceFile> Files { get; set; } = new List<AIServiceFile>();

        /// <summary>
        /// How to combine or change the source images.
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
        /// <exception cref="ArgumentNullException">Thrown when Prompt is null, empty or whitespace, or a file's stream or name is null.</exception>
        /// <exception cref="ArgumentException">Thrown when no files are provided.</exception>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Prompt))
                throw new ArgumentNullException(nameof(Prompt), "Prompt is required for image generation.");
            if (Files is null || Files.Count == 0)
                throw new ArgumentException("At least one source image is required.", nameof(Files));

            foreach (AIServiceFile file in Files)
            {
                if (file is null)
                    throw new ArgumentException("Source images cannot contain null entries.", nameof(Files));

                file.Validate();
            }
        }
    }
}
