using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using SerenityStar.Constants;
using SerenityStar.Models.Conversation;
using SerenityStar.Models.Execute;
using SerenityStar.Models.MessageFeedback;

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
            Conversation conversation = Conversation.CreateConversation(_httpClient, agentCode, null, options);
            if (!string.IsNullOrEmpty(conversationId))
                conversation.SetConversationId(conversationId);

            return conversation;
        }

        /// <summary>
        /// Creates a new conversation with a conversational agent using a specific version.
        /// The conversation is created automatically when the first message is sent.
        /// </summary>
        /// <param name="agentCode">The agent code.</param>
        /// <param name="version">The specific version of the agent to use.</param>
        /// <param name="conversationId">Optional existing conversation ID to resume.</param>
        /// <param name="options">Optional execution options.</param>
        /// <returns>A new conversation instance.</returns>
        public Conversation CreateConversation(
            string agentCode,
            int version,
            string? conversationId = null,
            AgentExecutionReq? options = null)
        {
            Conversation conversation = Conversation.CreateConversation(_httpClient, agentCode, version, options);
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
            Conversation conversation = new Conversation(_httpClient, agentCode, null, options);
            await conversation.InitializeInfoAsync(cancellationToken);
            return conversation.Info ?? throw new InvalidOperationException("Failed to get conversation info");
        }

        /// <summary>
        /// Gets information about a conversational agent with a specific version.
        /// </summary>
        /// <param name="agentCode">The agent code.</param>
        /// <param name="version">The specific version of the agent.</param>
        /// <param name="options">Optional execution options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Agent information.</returns>
        public async Task<ConversationInfoResult> GetInfoByCodeAsync(
            string agentCode,
            int version,
            AgentExecutionReq? options = null,
            CancellationToken cancellationToken = default)
        {
            Conversation conversation = new Conversation(_httpClient, agentCode, version, options);
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

        /// <summary>
        /// Gets a page of the feedback submitted for the agent's messages, including the comments.
        /// </summary>
        /// <param name="agentCode">The agent code.</param>
        /// <param name="options">Optional paging, date filtering and sorting options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A page of message feedback entries.</returns>
        /// <remarks>
        /// This operation requires an API key with the Audit permission for the agent, which is a
        /// stronger permission than the Execution one needed to submit or remove feedback. The returned
        /// entries expose end-user conversation content and user identifiers, so restrict access to them
        /// accordingly.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when the agent code is null or empty.</exception>
        /// <exception cref="HttpRequestException">
        /// Thrown when the API rejects the request, for example with an HTTP 400 response when
        /// <see cref="GetMessageFeedbackReq.PageSize"/> exceeds 1000 or
        /// <see cref="GetMessageFeedbackReq.SortDirection"/> is not "asc" or "desc".
        /// </exception>
        public async Task<MessageFeedbackPage> GetMessageFeedbackAsync(
            string agentCode,
            GetMessageFeedbackReq? options = null,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(agentCode))
                throw new ArgumentNullException(nameof(agentCode));

            GetMessageFeedbackReq request = options ?? new GetMessageFeedbackReq();

            List<string> queryParams = new List<string>
            {
                $"page={request.Page}",
                $"pageSize={request.PageSize}"
            };

            if (request.StartDate.HasValue)
                queryParams.Add($"startDate={FormatDate(request.StartDate.Value)}");

            if (request.EndDate.HasValue)
                queryParams.Add($"endDate={FormatDate(request.EndDate.Value)}");

            if (!string.IsNullOrWhiteSpace(request.SortDirection))
                queryParams.Add($"sortDirection={Uri.EscapeDataString(request.SortDirection)}");

            string url = $"/api/v2/agent/{Uri.EscapeDataString(agentCode)}/feedback?{string.Join("&", queryParams)}";

            HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
            }

            return await response.Content.ReadFromJsonAsync<MessageFeedbackPage>(JsonSerializerOptionsCache.s_camelCase, cancellationToken)
                   ?? throw new InvalidOperationException("Failed to deserialize message feedback page");
        }

        private static string FormatDate(DateTime value)
            => Uri.EscapeDataString(value.ToString("o", CultureInfo.InvariantCulture));
    }
}
