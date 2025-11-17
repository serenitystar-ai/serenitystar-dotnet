namespace SerenityStar.SDK.NET.Core.Models.Execute
{
    /// <summary>
    /// Represents the token usage information for an agent result.
    /// </summary>
    public class AgentResultTokenUsage
    {
        /// <summary>
        /// The number of tokens generated across all completions emissions.
        /// </summary>
        public int CompletionTokens { get; set; }

        /// <summary>
        /// The number of tokens in the provided prompts for the completions request.
        /// </summary>
        public int PromptTokens { get; set; }

        /// <summary>
        /// The total number of tokens processed for the completions request and response.
        /// </summary>
        public int TotalTokens => CompletionTokens + PromptTokens;
    }
}