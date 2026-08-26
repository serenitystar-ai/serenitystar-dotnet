using SerenityStar.Agents.VolatileKnowledge;
using SerenityStar.Constants;
using SerenityStar.Helpers;
using SerenityStar.Models.Connector;
using SerenityStar.Models.Conversation;
using SerenityStar.Models.Execute;
using SerenityStar.Models.MessageFeedback;
using SerenityStar.Models.Streaming;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SerenityStar.Agents.Conversational
{
    /// <summary>
    /// Represents a conversation with an assistant agent.
    /// </summary>
    public sealed class Conversation
    {
        private readonly HttpClient _httpClient;
        private readonly string _agentCode;
        private readonly int? _version;
        private readonly AgentExecutionReq? _options;
        private string? _chatId;

        /// <summary>
        /// The conversation ID (instanceId from the first message response).
        /// </summary>
        public string? ConversationId => _chatId;

        /// <summary>
        /// Information about the conversation.
        /// </summary>
        public ConversationInfoResult? Info { get; private set; }

        /// <summary>
        /// Provides methods for managing volatile knowledge within this conversation.
        /// Uploaded knowledge is automatically associated with this conversation.
        /// </summary>
        public ConversationVolatileKnowledgeScope VolatileKnowledge { get; }

        internal Conversation(HttpClient httpClient, string agentCode, int? version = null, AgentExecutionReq? options = null)
        {
            _httpClient = httpClient;
            _agentCode = agentCode;
            _version = version;
            _options = options;
            VolatileKnowledge = new ConversationVolatileKnowledgeScope(httpClient, agentCode);
        }

        /// <summary>
        /// Sets the conversation ID (for resuming existing conversations).
        /// </summary>
        /// <param name="conversationId">The conversation ID to set.</param>
        internal void SetConversationId(string conversationId)
        {
            _chatId = conversationId;
        }

        /// <summary>
        /// Creates a new conversation instance for the specified assistant agent.
        /// The conversation is created automatically when the first message is sent.
        /// </summary>
        /// <param name="httpClient">The HTTP client to use for API calls.</param>
        /// <param name="agentCode">The assistant agent code.</param>
        /// <param name="version">Optional specific version of the agent. If not specified, uses the published version.</param>
        /// <param name="options">Optional execution options.</param>
        /// <returns>A new conversation instance.</returns>
        public static Conversation CreateConversation(HttpClient httpClient, string agentCode, int? version = null, AgentExecutionReq? options = null)
            => new Conversation(httpClient, agentCode, version, options);

        internal async Task InitializeInfoAsync(CancellationToken cancellationToken = default)
        {
            string versionPath = _version.HasValue ? $"/{_version.Value}" : string.Empty;
            string url = $"/api/v2/agent/{_agentCode}{versionPath}/conversation/info";

            Dictionary<string, object?> requestBody = new Dictionary<string, object?>
            {
                ["inputParameters"] = _options?.InputParameters
            };

            if (_options?.UserIdentifier != null)
                requestBody["userIdentifier"] = _options.UserIdentifier;
            if (_options?.Channel != null)
                requestBody["channel"] = _options.Channel;

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, requestBody, JsonSerializerOptionsCache.s_camelCase, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
            }

            Info = await response.Content.ReadFromJsonAsync<ConversationInfoResult>(JsonSerializerOptionsCache.s_camelCase, cancellationToken)
                   ?? throw new InvalidOperationException("Failed to deserialize conversation info");
        }

        /// <summary>
        /// Sends a text message in the conversation.
        /// The conversation is created automatically on the first message.
        /// Subsequent messages use the instanceId from the first response as chatId.
        /// </summary>
        /// <param name="message">The text message to send.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The agent's response.</returns>
        public Task<AgentResult> SendMessageAsync(string message, CancellationToken cancellationToken = default)
            => SendMessageCoreAsync(message, null, null, cancellationToken);

        /// <summary>
        /// Sends an audio file in the conversation. The SDK uploads the file and the agent transcribes the audio internally.
        /// The conversation is created automatically on the first message.
        /// Subsequent messages use the instanceId from the first response as chatId.
        /// </summary>
        /// <param name="audioStream">The audio file stream to upload and send as input.</param>
        /// <param name="audioFileName">The file name including extension (e.g., "recording.mp3").</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The agent's response.</returns>
        public Task<AgentResult> SendMessageAsync(Stream audioStream, string audioFileName, CancellationToken cancellationToken = default)
            => SendMessageCoreAsync(null, audioStream, audioFileName, cancellationToken);

        private async Task<AgentResult> SendMessageCoreAsync(string? message, Stream? audioStream, string? audioFileName, CancellationToken cancellationToken)
        {
            string url = _version.HasValue
                ? $"/api/v2/agent/{_agentCode}/execute/{_version.Value}"
                : $"/api/v2/agent/{_agentCode}/execute";

            List<object> parameters = await BuildParametersAsync(message, audioStream, audioFileName, isStream: false, cancellationToken);

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, parameters, JsonSerializerOptionsCache.s_camelCase, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
            }

            AgentResult result = await response.Content.ReadFromJsonAsync<AgentResult>(JsonSerializerOptionsCache.s_camelCase, cancellationToken)
                   ?? throw new InvalidOperationException("Failed to deserialize response");

            // Store instanceId as chatId for subsequent messages
            if (string.IsNullOrEmpty(_chatId) && result.InstanceId != Guid.Empty)
                _chatId = result.InstanceId.ToString();

            // Clear volatile knowledge IDs after sending message
            VolatileKnowledge.ClearKnowledgeIds();

            return result;
        }

        /// <summary>
        /// Streams a text message in the conversation.
        /// The conversation is created automatically on the first message.
        /// Subsequent messages use the instanceId from the first response as chatId.
        /// </summary>
        /// <param name="message">The text message to send.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>An async enumerable of streaming messages.</returns>
        public IAsyncEnumerable<StreamingAgentMessage> StreamMessageAsync(string message, CancellationToken cancellationToken = default)
            => StreamMessageCoreAsync(message, null, null, cancellationToken);

        /// <summary>
        /// Streams an audio file in the conversation. The SDK uploads the file and the agent transcribes the audio internally.
        /// The conversation is created automatically on the first message.
        /// Subsequent messages use the instanceId from the first response as chatId.
        /// </summary>
        /// <param name="audioStream">The audio file stream to upload and send as input.</param>
        /// <param name="audioFileName">The file name including extension (e.g., "recording.mp3").</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>An async enumerable of streaming messages.</returns>
        public IAsyncEnumerable<StreamingAgentMessage> StreamMessageAsync(Stream audioStream, string audioFileName, CancellationToken cancellationToken = default)
            => StreamMessageCoreAsync(null, audioStream, audioFileName, cancellationToken);

        private async IAsyncEnumerable<StreamingAgentMessage> StreamMessageCoreAsync(
            string? message,
            Stream? audioStream,
            string? audioFileName,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            string url = _version.HasValue
                ? $"/api/v2/agent/{_agentCode}/execute/{_version.Value}"
                : $"/api/v2/agent/{_agentCode}/execute";

            List<object> parameters = await BuildParametersAsync(message, audioStream, audioFileName, isStream: true, cancellationToken);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(parameters, options: JsonSerializerOptionsCache.s_camelCase)
            };

            HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
            }

            using (Stream stream = await response.Content.ReadAsStreamAsync())
            using (StreamReader reader = new StreamReader(stream))
            {
                bool messageStarted = false;
                while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
                {
                    string? line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
                        continue;

                    string data = line.Substring(6).Trim();
                    if (data == "[DONE]")
                        break;

                    StreamingAgentMessage? msg = JsonSerializer.Deserialize<StreamingAgentMessage>(data, JsonSerializerOptionsCache.s_streamingSnakeCaseLower);
                    if (msg != null)
                    {
                        if (!messageStarted)
                        {
                            messageStarted = true;
                            // Clear volatile knowledge IDs when streaming starts
                            VolatileKnowledge.ClearKnowledgeIds();
                        }

                        // Store conversationId from the stop message
                        if (msg is StreamingAgentMessageStop stop && string.IsNullOrEmpty(_chatId))
                        {
                            // Get instanceId from the result object
                            if (stop.Result?.InstanceId != Guid.Empty && stop.Result?.InstanceId != null)
                                _chatId = stop.Result.InstanceId.ToString();
                        }
                        yield return msg;
                    }
                }
            }
        }

        private async Task<List<object>> BuildParametersAsync(string? message, Stream? audioStream, string? audioFileName, bool isStream, CancellationToken cancellationToken)
        {
            List<object> parameters = new();

            if (isStream)
                parameters.Add(new { Key = "stream", Value = true });

            if (message != null)
                parameters.Add(new { Key = "message", Value = message });

            if (audioStream != null)
            {
                Guid uploadedFileId = await FileUploadHelper.UploadFileAsync(_httpClient, audioStream, audioFileName!, cancellationToken);
                parameters.Add(new { Key = "audioInput", Value = JsonSerializer.Serialize(new { fileId = uploadedFileId }, JsonSerializerOptionsCache.s_camelCase) });
            }

            // Add chatId only if we have it from a previous message
            if (!string.IsNullOrEmpty(_chatId))
                parameters.Insert(0, new { Key = "chatId", Value = _chatId });

            if (_options?.InputParameters != null)
                foreach (KeyValuePair<string, object> param in _options.InputParameters)
                    parameters.Add(new { param.Key, param.Value });

            if (_options?.UserIdentifier != null)
                parameters.Add(new { Key = "userIdentifier", Value = _options.UserIdentifier });

            if (_options?.Channel != null)
                parameters.Add(new { Key = "channel", Value = _options.Channel });

            // Add volatile knowledge IDs if any are associated
            if (VolatileKnowledge.KnowledgeIds.Any())
                parameters.Add(new { Key = "volatileKnowledgeIds", Value = VolatileKnowledge.KnowledgeIds.Select(id => id.ToString()).ToList() });

            return parameters;
        }

        /// <summary>
        /// Gets a conversation by ID.
        /// </summary>
        /// <param name="conversationId">The conversation ID.</param>
        /// <param name="showExecutorTaskLogs">Whether to include executor task logs.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The conversation details.</returns>
        public async Task<ConversationRes> GetConversationByIdAsync(
            string conversationId,
            bool showExecutorTaskLogs = false,
            CancellationToken cancellationToken = default)
        {
            string versionPath = _version.HasValue ? $"/{_version.Value}" : string.Empty;
            string url = $"/api/v2/agent/{_agentCode}/conversation/{conversationId}{versionPath}?showExecutorTaskLogs={showExecutorTaskLogs}";

            HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
            }

            return await response.Content.ReadFromJsonAsync<ConversationRes>(JsonSerializerOptionsCache.s_camelCase, cancellationToken)
                   ?? throw new InvalidOperationException("Failed to deserialize conversation details");
        }

        /// <summary>
        /// Submits feedback for a message.
        /// </summary>
        /// <param name="options">The feedback options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <remarks>
        /// Submitting feedback again for the same message overwrites the previous feedback, including
        /// its comment. Leaving <see cref="SubmitFeedbackReq.Comment"/> null clears a comment that was
        /// submitted previously.
        /// </remarks>
        public async Task SubmitFeedbackAsync(SubmitFeedbackReq options, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(ConversationId))
                throw new InvalidOperationException("Conversation not initialized");

            string url = $"/api/v2/agent/{_agentCode}/conversation/{ConversationId}/message/{options.AgentMessageId}/feedback";

            Dictionary<string, object> requestBody = new()
            {
                ["feedback"] = options.Feedback
            };

            // The comment length limit is enforced by the API, which responds with 400 when exceeded.
            if (!string.IsNullOrWhiteSpace(options.Comment))
                requestBody["comment"] = options.Comment!;

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, requestBody, JsonSerializerOptionsCache.s_camelCase, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
            }
        }

        /// <summary>
        /// Removes feedback from a message.
        /// </summary>
        /// <param name="options">The remove feedback options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async Task RemoveFeedbackAsync(RemoveFeedbackReq options, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(ConversationId))
                throw new InvalidOperationException("Conversation not initialized");

            string url = $"/api/v2/agent/{_agentCode}/conversation/{ConversationId}/message/{options.AgentMessageId}/feedback";

            HttpResponseMessage response = await _httpClient.DeleteAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
            }
        }

        /// <summary>
        /// Gets the connector status for this conversation.
        /// </summary>
        /// <param name="connectorId">The connector ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The connector status.</returns>
        public async Task<ConnectorStatusRes> GetConnectorStatusAsync(
            Guid connectorId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(ConversationId))
                throw new InvalidOperationException("Conversation not initialized");

            string url = $"/api/v2/connection/agentInstance/{ConversationId}/connector/{connectorId}/status";
            HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
            }

            return await response.Content.ReadFromJsonAsync<ConnectorStatusRes>(JsonSerializerOptionsCache.s_camelCase, cancellationToken)
                   ?? throw new InvalidOperationException("Failed to deserialize connector status");
        }
    }
}
