using SerenityStar.Models.AIServices;
using System;

namespace SerenityStar.Models.ImageGeneration
{
    /// <summary>
    /// Represents the result of an image generation operation.
    /// </summary>
    public sealed class ImageGenerationResult
    {
        /// <summary>
        /// The unique identifier of the image generation instance.
        /// </summary>
        public Guid InstanceId { get; set; }

        /// <summary>
        /// The public URL to download the generated image.
        /// </summary>
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// The billed quantities in the units the model is priced by.
        /// </summary>
        public AIServiceUsage? Usage { get; set; }

        /// <summary>
        /// The cost information for this execution in the tenant's currency.
        /// </summary>
        public AIServiceCost? Cost { get; set; }
    }
}
