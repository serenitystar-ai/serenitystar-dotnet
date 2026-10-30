using SerenityStar.Models.AIServices;
using System;

namespace SerenityStar.Models.Vision
{
    /// <summary>
    /// Represents the result of an image analysis operation.
    /// </summary>
    public sealed class VisionResult
    {
        /// <summary>
        /// The unique identifier of the vision instance.
        /// </summary>
        public Guid InstanceId { get; set; }

        /// <summary>
        /// The model's analysis of the image.
        /// </summary>
        public string Response { get; set; } = string.Empty;

        /// <summary>
        /// The billed quantities in the units the model is priced by.
        /// </summary>
        public AIServiceUsage? Usage { get; set; }

        /// <summary>
        /// The cost information for this execution in the tenant's currency.
        /// </summary>
        public AIServiceTokenCost? Cost { get; set; }
    }
}
