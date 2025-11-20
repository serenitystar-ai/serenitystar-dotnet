using SerenityStar.Agents.VolatileKnowledge;
using SerenityStar.Models.ChatCompletion;
using SerenityStar.Models.Execute;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace SerenityStar.Agents.System
{
    /// <summary>
    /// Represents a ChatCompletion agent.
    /// </summary>
    public sealed class ChatCompletion : SystemAgentBase
    {
        private readonly ChatCompletionReq _chatOptions;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Provides methods for managing volatile knowledge within this chat completion.
        /// Uploaded knowledge is automatically associated with this chat completion.
        /// </summary>
        public ConversationVolatileKnowledgeScope VolatileKnowledge { get; }

        internal ChatCompletion(HttpClient httpClient, string agentCode, ChatCompletionReq options)
            : base(httpClient, agentCode, options)
        {
            _chatOptions = options;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            VolatileKnowledge = new ConversationVolatileKnowledgeScope(httpClient);
        }

        /// <inheritdoc/>
        protected override object CreateExecuteBody(bool stream)
        {
            List<object> parameters = CreateBaseParameters(stream);

            // Add message
            parameters.Add(new { Key = "message", Value = _chatOptions.Message });

            // Add messages if provided
            if (_chatOptions.Messages != null && _chatOptions.Messages.Count > 0)
                parameters.Add(new { Key = "messages", Value = JsonSerializer.Serialize(_chatOptions.Messages, _jsonOptions) });

            // Add input parameters if provided
            if (_chatOptions.InputParameters != null)
                foreach (KeyValuePair<string, object> param in _chatOptions.InputParameters)
                    parameters.Add(new { param.Key, param.Value });

            // Add volatile knowledge IDs if any are associated
            if (VolatileKnowledge.KnowledgeIds.Any())
                parameters.Add(new { Key = "volatileKnowledgeIds", Value = VolatileKnowledge.KnowledgeIds.Select(id => id.ToString()).ToList() });

            return parameters;
        }

        /// <inheritdoc/>
        protected override void OnExecutionComplete()
        {
            // Clear volatile knowledge IDs after execution
            VolatileKnowledge.ClearKnowledgeIds();
        }
    }

    /// <summary>
    /// Provides methods for interacting with ChatCompletion agents.
    /// </summary>
    public sealed class ChatCompletionsScope
    {
        private readonly HttpClient _httpClient;

        internal ChatCompletionsScope(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Executes a chat completion agent.
        /// </summary>
        /// <param name="agentCode">The agent code.</param>
        /// <param name="options">Chat completion options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task<AgentResult> ExecuteAsync(
            string agentCode,
            ChatCompletionReq options,
            CancellationToken cancellationToken = default)
        {
            ChatCompletion chatCompletion = new ChatCompletion(_httpClient, agentCode, options);
            return chatCompletion.ExecuteAsync(cancellationToken);
        }

        /// <summary>
        /// Creates a chat completion agent for streaming.
        /// </summary>
        /// <param name="agentCode">The agent code.</param>
        /// <param name="options">Chat completion options.</param>
        /// <returns>A chat completion instance for streaming.</returns>
        public ChatCompletion Create(string agentCode, ChatCompletionReq options)
        {
            return new ChatCompletion(_httpClient, agentCode, options);
        }
    }
}
