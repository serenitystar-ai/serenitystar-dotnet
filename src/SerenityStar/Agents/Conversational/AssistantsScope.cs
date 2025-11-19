using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SerenityStar.Models.Conversation;
using SerenityStar.Models.Execute;

namespace SerenityStar.Agents.Conversational
{
    /// <summary>
    /// Provides methods for interacting with Assistant agents.
    /// </summary>
    public class AssistantsScope
    {
        private readonly HttpClient _httpClient;

        internal AssistantsScope(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Gets information about an assistant agent.
        /// </summary>
        /// <param name="agentCode">The agent code.</param>
        /// <param name="options">Optional execution options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Agent information.</returns>
        public async Task<ConversationInfoResult> GetInfoByCodeAsync(
            string agentCode,
            AgentExecutionOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            Conversation conversation = new Conversation(_httpClient, agentCode, options);
            await conversation.InitializeInfoAsync(cancellationToken);
            return conversation.Info ?? throw new InvalidOperationException("Failed to get conversation info");
        }

        /// <summary>
        /// Gets a conversation by ID.
        /// </summary>
        /// <param name="agentCode">The agent code.</param>
        /// <param name="conversationId">The conversation ID.</param>
        /// <param name="showExecutorTaskLogs">Whether to include executor task logs.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The conversation details.</returns>
        public async Task<ConversationDetails> GetConversationByIdAsync(
            string agentCode,
            string conversationId,
            bool showExecutorTaskLogs = false,
            CancellationToken cancellationToken = default)
        {
            Conversation conversation = new Conversation(_httpClient, agentCode);
            return await conversation.GetConversationByIdAsync(conversationId, showExecutorTaskLogs, cancellationToken);
        }
    }
}
