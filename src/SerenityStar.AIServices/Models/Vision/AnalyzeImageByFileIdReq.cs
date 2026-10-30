using System;

namespace SerenityStar.Models.Vision
{
    /// <summary>
    /// Request for analyzing an image previously uploaded to Serenity Star.
    /// </summary>
    /// <remarks>
    /// The model is optional: set <see cref="AIModelId"/>, or both <see cref="ModelCode"/> and <see cref="Vendor"/>,
    /// or neither to use the default vision model.
    /// </remarks>
    public sealed class AnalyzeImageByFileIdReq
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
        /// The id of the uploaded image to analyze.
        /// </summary>
        public Guid FileId { get; set; }

        /// <summary>
        /// Optional instructions about what to analyze in the image.
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
        /// <exception cref="ArgumentException">Thrown when FileId is empty.</exception>
        public void Validate()
        {
            if (FileId == Guid.Empty)
                throw new ArgumentException("File id is required for image analysis.", nameof(FileId));
        }
    }
}
