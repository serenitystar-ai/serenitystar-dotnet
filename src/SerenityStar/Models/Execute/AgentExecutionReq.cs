using System.Collections.Generic;
using System.IO;

namespace SerenityStar.Models.Execute
{
    /// <summary>
    /// Options for executing an agent.
    /// </summary>
    public sealed class AgentExecutionReq
    {
        /// <summary>
        /// Input parameters for the agent execution.
        /// </summary>
        public Dictionary<string, object>? InputParameters { get; set; }

        /// <summary>
        /// User identifier for tracking and personalization.
        /// </summary>
        public string? UserIdentifier { get; set; }

        /// <summary>
        /// Channel identifier for the execution context.
        /// </summary>
        public string? Channel { get; set; }

        /// <summary>
        /// Optional audio file stream to upload and send as input.
        /// The SDK uploads the file automatically and sends the resulting file ID to the agent.
        /// The agent must have audio input support enabled.
        /// </summary>
        public Stream? AudioFileStream { get; set; }

        /// <summary>
        /// The file name including extension for the audio file (e.g., "recording.mp3").
        /// Required when AudioFileStream is provided.
        /// </summary>
        public string? AudioFileName { get; set; }
    }
}
