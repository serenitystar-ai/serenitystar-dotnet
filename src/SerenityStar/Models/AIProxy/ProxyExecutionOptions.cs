using System.Collections.Generic;

namespace SerenityStar.Models.AIProxy
{
    /// <summary>
    /// Options for executing a proxy agent.
    /// </summary>
    public class ProxyExecutionOptions
    {
        /// <summary>
        /// The model to use for the execution.
        /// </summary>
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// The messages to send to the model.
        /// </summary>
        public List<ProxyExecutionMessage> Messages { get; set; } = new List<ProxyExecutionMessage>();

        /// <summary>
        /// Temperature for response randomness (0.0 to 2.0).
        /// </summary>
        public double? Temperature { get; set; }

        /// <summary>
        /// Maximum number of tokens to generate.
        /// </summary>
        public int? MaxTokens { get; set; }

        /// <summary>
        /// Top-p sampling parameter.
        /// </summary>
        public double? TopP { get; set; }

        /// <summary>
        /// Top-k sampling parameter.
        /// </summary>
        public int? TopK { get; set; }

        /// <summary>
        /// Frequency penalty parameter.
        /// </summary>
        public double? FrequencyPenalty { get; set; }

        /// <summary>
        /// Presence penalty parameter.
        /// </summary>
        public double? PresencePenalty { get; set; }

        /// <summary>
        /// User identifier for tracking.
        /// </summary>
        public string? UserIdentifier { get; set; }

        /// <summary>
        /// Vendor identifier.
        /// </summary>
        public string? Vendor { get; set; }

        /// <summary>
        /// Group identifier.
        /// </summary>
        public string? GroupIdentifier { get; set; }

        /// <summary>
        /// Indicates if vision capabilities should be used.
        /// </summary>
        public bool? UseVision { get; set; }
    }
}
