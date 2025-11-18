using SerenityStar.Models.MessageFeedback;
using SerenityStar.Models.Execute;
using SerenityStar.Models.Connector;
using System.Net.Http.Json;

namespace SerenityStar.Models.Conversation
{
    public class Conversation
    {
        // ...existing properties and methods...

        /// <summary>
        /// Submits feedback for an agent message.
        /// </summary>
        /// <param name="options">The feedback options including agent message ID and feedback value</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task SubmitFeedbackAsync(SubmitFeedbackOptions options, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(ConversationId))
                throw new InvalidOperationException("Cannot submit feedback before a conversation has been created. Send a message first.");

            if (options.AgentMessageId == Guid.Empty)
                throw new ArgumentException("Agent message ID is required.", nameof(options.AgentMessageId));

            string endpoint = $"/api/agent/{_agentCode}/conversation/{ConversationId}/message/{options.AgentMessageId}/feedback";
            MessageFeedbackRequest request = new MessageFeedbackRequest { Feedback = options.Feedback };

            await _httpClient.PostAsJsonAsync(endpoint, request, cancellationToken);
        }

        /// <summary>
        /// Removes feedback for an agent message.
        /// </summary>
        /// <param name="options">The options including agent message ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task RemoveFeedbackAsync(RemoveFeedbackOptions options, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(ConversationId))
                throw new InvalidOperationException("Cannot remove feedback before a conversation has been created. Send a message first.");

            if (options.AgentMessageId == Guid.Empty)
                throw new ArgumentException("Agent message ID is required.", nameof(options.AgentMessageId));

            string endpoint = $"/api/agent/{_agentCode}/conversation/{ConversationId}/message/{options.AgentMessageId}/feedback";

            await _httpClient.DeleteAsync(endpoint, cancellationToken);
        }

        /// <summary>
        /// Gets the connection status of a connector for the current conversation.
        /// </summary>
        /// <param name="options">The options including agent instance ID and connector ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The connector status response</returns>
        public async Task<ConnectorStatusResponse> GetConnectorStatusAsync(GetConnectorStatusOptions options, CancellationToken cancellationToken = default)
        {
            if (options.AgentInstanceId == Guid.Empty)
                throw new ArgumentException("Agent instance ID is required.", nameof(options.AgentInstanceId));

            if (options.ConnectorId == Guid.Empty)
                throw new ArgumentException("Connector ID is required.", nameof(options.ConnectorId));

            string endpoint = $"/api/connection/agentInstance/{options.AgentInstanceId}/connector/{options.ConnectorId}/status";

            HttpResponseMessage response = await _httpClient.GetAsync(endpoint, cancellationToken);
            response.EnsureSuccessStatusCode();

            ConnectorStatusResponse? result = await response.Content.ReadFromJsonAsync<ConnectorStatusResponse>(cancellationToken);
            return result ?? throw new InvalidOperationException("Failed to deserialize connector status response.");
        }
    }
}