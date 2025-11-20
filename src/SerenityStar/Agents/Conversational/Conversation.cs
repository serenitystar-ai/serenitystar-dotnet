using SerenityStar.Models.Conversation;
using SerenityStar.Models.Execute;
using SerenityStar.Models.MessageFeedback;
using SerenityStar.Models.Streaming;
using System;
using System.Collections.Generic;
using System.IO;
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
        private readonly AgentExecutionReq? _options;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly JsonSerializerOptions _snakeCaseJsonOptions;
        private string? _chatId;

        /// <summary>
        /// The conversation ID (instanceId from the first message response).
        /// </summary>
        public string? ConversationId => _chatId;

        /// <summary>
        /// Information about the conversation.
        /// </summary>
        public ConversationInfoResult? Info { get; private set; }

        internal Conversation(HttpClient httpClient, string agentCode, AgentExecutionReq? options = null)
        {
            _httpClient = httpClient;
            _agentCode = agentCode;
            _options = options;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            _snakeCaseJsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            };
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
        /// <param name="options">Optional execution options.</param>
        /// <returns>A new conversation instance.</returns>
        public static Conversation CreateConversation(HttpClient httpClient, string agentCode, AgentExecutionReq? options = null)
            => new Conversation(httpClient, agentCode, options);

        internal async Task InitializeInfoAsync(CancellationToken cancellationToken = default)
        {
            string version = _options?.AgentVersion.HasValue == true ? $"/{_options.AgentVersion}" : string.Empty;
            string url = $"/api/v2/agent/{_agentCode}/{version}/conversation/info";

            Dictionary<string, object?> requestBody = new Dictionary<string, object?>
            {
                ["inputParameters"] = _options?.InputParameters
            };

            if (_options?.UserIdentifier != null)
                requestBody["userIdentifier"] = _options.UserIdentifier;
            if (_options?.Channel != null)
                requestBody["channel"] = _options.Channel;

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, requestBody, _jsonOptions, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
            }

            Info = await response.Content.ReadFromJsonAsync<ConversationInfoResult>(_jsonOptions, cancellationToken)
                   ?? throw new InvalidOperationException("Failed to deserialize conversation info");
        }

        /// <summary>
        /// Sends a message in the conversation.
        /// The conversation is created automatically on the first message.
        /// Subsequent messages use the instanceId from the first response as chatId.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The agent's response.</returns>
        public async Task<AgentResult> SendMessageAsync(string message, CancellationToken cancellationToken = default)
        {
            string version = _options?.AgentVersion.HasValue == true ? $"/{_options.AgentVersion}" : string.Empty;
            string url = $"/api/v2/agent/{_agentCode}/execute{version}";

            List<object> parameters = new List<object>
            {
                new { Key = "message", Value = message }
            };

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

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, parameters, _jsonOptions, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
            }

            AgentResult result = await response.Content.ReadFromJsonAsync<AgentResult>(_jsonOptions, cancellationToken)
                   ?? throw new InvalidOperationException("Failed to deserialize response");

            // Store instanceId as chatId for subsequent messages
            if (string.IsNullOrEmpty(_chatId) && result.InstanceId != Guid.Empty)
                _chatId = result.InstanceId.ToString();

            return result;
        }

        /// <summary>
        /// Streams a message in the conversation.
        /// The conversation is created automatically on the first message.
        /// Subsequent messages use the instanceId from the first response as chatId.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>An async enumerable of streaming messages.</returns>
        public async IAsyncEnumerable<StreamingAgentMessage> StreamMessageAsync(
            string message,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            string version = _options?.AgentVersion.HasValue == true ? $"/{_options.AgentVersion}" : string.Empty;
            string url = $"/api/v2/agent/{_agentCode}/execute{version}";

            List<object> parameters = new List<object>
            {
                new { Key = "message", Value = message },
                new { Key = "stream", Value = true }
            };

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

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(parameters, options: _jsonOptions)
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
                yield return new StreamingAgentMessageStart();

                while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
                {
                    string? line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
                        continue;

                    string data = line.Substring(6).Trim();
                    if (data == "[DONE]")
                        break;

                    StreamingAgentMessage? msg = ParseStreamingMessage(data);
                    if (msg != null)
                    {
                        // Store conversationId from the stop message
                        if (msg is StreamingAgentMessageStop stop && string.IsNullOrEmpty(_chatId))
                        {
                            // Try to get instanceId from the result object
                            if (stop.Result?.InstanceId != Guid.Empty && stop.Result?.InstanceId != null)
                                _chatId = stop.Result.InstanceId.ToString();
                            // Fallback to direct InstanceId property if available
                            else if (stop.InstanceId.HasValue && stop.InstanceId.Value != Guid.Empty)
                                _chatId = stop.InstanceId.Value.ToString();
                        }
                        yield return msg;
                    }
                }
            }
        }

        private StreamingAgentMessage? ParseStreamingMessage(string json)
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                if (!root.TryGetProperty("type", out JsonElement typeElement))
                    return null;

                string type = typeElement.GetString() ?? string.Empty;

                return type switch
                {
                    "task_start" => new StreamingAgentMessageTaskStart
                    {
                        Key = root.GetProperty("key").GetString() ?? string.Empty,
                        Input = root.GetProperty("input").GetString() ?? string.Empty
                    },
                    "content" => new StreamingAgentMessageContent
                    {
                        Text = root.GetProperty("text").GetString() ?? string.Empty
                    },
                    "task_end" => new StreamingAgentMessageTaskEnd
                    {
                        Key = root.GetProperty("key").GetString() ?? string.Empty,
                        Result = root.TryGetProperty("result", out JsonElement resultElem) ? resultElem.Clone() : null,
                        DurationMs = root.TryGetProperty("duration", out JsonElement durationElem) ? durationElem.GetInt64() : 0
                    },
                    "stop" => JsonSerializer.Deserialize<StreamingAgentMessageStop>(json, _snakeCaseJsonOptions),
                    "error" => new StreamingAgentMessageError
                    {
                        Error = root.GetProperty("error").GetString() ?? string.Empty,
                        StatusCode = root.TryGetProperty("statusCode", out JsonElement statusElem) ? statusElem.GetInt32() : null
                    },
                    _ => null
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a conversation by ID.
        /// </summary>
        /// <param name="conversationId">The conversation ID.</param>
        /// <param name="showExecutorTaskLogs">Whether to include executor task logs.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The conversation details.</returns>
        public async Task<ConversationDetails> GetConversationByIdAsync(
            string conversationId,
            bool showExecutorTaskLogs = false,
            CancellationToken cancellationToken = default)
        {
            string version = _options?.AgentVersion.HasValue == true ? $"/{_options.AgentVersion}" : string.Empty;
            string url = $"/api/v2/agent/{_agentCode}/conversation/{conversationId}{version}?showExecutorTaskLogs={showExecutorTaskLogs}";

            HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
            }

            return await response.Content.ReadFromJsonAsync<ConversationDetails>(_jsonOptions, cancellationToken)
                   ?? throw new InvalidOperationException("Failed to deserialize conversation details");
        }

        /// <summary>
        /// Submits feedback for a message.
        /// </summary>
        /// <param name="options">The feedback options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public async Task SubmitFeedbackAsync(SubmitFeedbackReq options, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(ConversationId))
                throw new InvalidOperationException("Conversation not initialized");

            string url = $"/api/v2/agent/{_agentCode}/conversation/{ConversationId}/message/{options.AgentMessageId}/feedback";

            Dictionary<string, object> requestBody = new Dictionary<string, object>
            {
                ["feedback"] = options.Feedback
            };

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, requestBody, _jsonOptions, cancellationToken);

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
    }
}
