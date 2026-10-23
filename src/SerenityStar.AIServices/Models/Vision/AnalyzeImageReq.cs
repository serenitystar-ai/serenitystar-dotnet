using System;
using System.IO;

namespace SerenityStar.Models.Vision
{
    /// <summary>
    /// Request for analyzing an image by uploading it directly.
    /// </summary>
    /// <remarks>
    /// The model is optional: set <see cref="AIModelId"/>, or both <see cref="ModelCode"/> and <see cref="Vendor"/>,
    /// or neither to use the default vision model.
    /// </remarks>
    public sealed class AnalyzeImageReq
    {
        /// <summary>
        /// The id of the AI Model to use. Do not combine with <see cref="ModelCode"/> and <see cref="Vendor"/>.
        /// </summary>
        public Guid? AIModelId { get; set; }

        /// <summary>
        /// The code of the model to use (e.g. "gpt-4o"). Must be combined with <see cref="Vendor"/>.
        /// </summary>
        public string? ModelCode { get; set; }

        /// <summary>
        /// The vendor of the model identified by <see cref="ModelCode"/> (e.g. "OpenAI").
        /// </summary>
        public string? Vendor { get; set; }

        /// <summary>
        /// The image file stream to analyze.
        /// Supported formats: jpg, jpeg, png, gif, webp.
        /// Maximum file size: 20 MB.
        /// </summary>
        public Stream FileStream { get; set; } = null!;

        /// <summary>
        /// The file name including extension (e.g., "photo.png").
        /// </summary>
        public string FileName { get; set; } = null!;

        /// <summary>
        /// Optional instructions about what to analyze in the image. Maximum length: 4000 characters.
        /// </summary>
        public string? Prompt { get; set; }

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
        /// <exception cref="ArgumentNullException">Thrown when FileStream or FileName is null.</exception>
        public void Validate()
        {
            if (FileStream is null)
                throw new ArgumentNullException(nameof(FileStream), "File stream is required for image analysis.");
            if (string.IsNullOrEmpty(FileName))
                throw new ArgumentNullException(nameof(FileName), "File name is required for image analysis.");
        }
    }
}
