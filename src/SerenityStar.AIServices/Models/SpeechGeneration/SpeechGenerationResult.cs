using SerenityStar.Models.AIServices;
using System;

namespace SerenityStar.Models.SpeechGeneration
{
    /// <summary>
    /// Represents the result of a speech generation operation.
    /// </summary>
    public sealed class SpeechGenerationResult
    {
        /// <summary>
        /// The unique identifier of the speech generation instance.
        /// </summary>
        public Guid InstanceId { get; set; }

        /// <summary>
        /// The public URL to download the generated audio. It doesn't require authentication.
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
    }
}
