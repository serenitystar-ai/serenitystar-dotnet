using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SerenityStar.Models.Conversation;
using SerenityStar.Models.Execute;

namespace SerenityStar.Agents.Conversational
{
    /// <summary>
    /// Base class for conversational agents (Assistants, Copilots).
    /// </summary>
    public abstract class ConversationalAgentBase
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Initializes a new instance of the ConversationalAgentBase class.
        /// </summary>
        protected ConversationalAgentBase(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Creates a new conversation with a conversational agent.
        /// The conversation is created automatically when the first message is sent.
        /// </summary>
        /// <param name="agentCode">The agent code.</param>
        /// <param name="conversationId">Optional existing conversation ID to resume.</param>
        /// <param name="options">Optional execution options.</param>
        /// <returns>A new conversation instance.</returns>
        public Conversation CreateConversation(
            string agentCode,
            string? conversationId = null,
            AgentExecutionReq? options = null)
        {
            Conversation conversation = Conversation.CreateConversation(_httpClient, agentCode, options);
            if (!string.IsNullOrEmpty(conversationId))
                conversation.SetConversationId(conversationId);

            return conversation;
        }

        /// <summary>
        /// Gets information about a conversational agent.
        /// </summary>
        /// <param name="agentCode">The agent code.</param>
        /// <param name="options">Optional execution options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Agent information.</returns>
        public async Task<ConversationInfoResult> GetInfoByCodeAsync(
            string agentCode,
            AgentExecutionReq? options = null,
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
        public async Task<ConversationRes> GetConversationByIdAsync(
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
