using System;
using System.Collections.Generic;

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
        /// Optional file ID of a previously uploaded audio file.
        /// When set, the agent will transcribe the audio and use the transcription as input.
        /// The agent must have audio input support enabled.
        /// </summary>
        public Guid? AudioFileId { get; set; }
    }
}
