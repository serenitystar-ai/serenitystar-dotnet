using SerenityStar.Agents.VolatileKnowledge;
using SerenityStar.Constants;
using SerenityStar.Models.AIProxy;
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
    /// Represents a Proxy agent.
    /// </summary>
    public sealed class Proxy : SystemAgentBase
    {
        private readonly ProxyExecutionReq _proxyOptions;

        /// <summary>
        /// Provides methods for managing volatile knowledge within this proxy.
        /// Uploaded knowledge is automatically associated with this proxy.
        /// </summary>
        public ConversationVolatileKnowledgeScope VolatileKnowledge { get; }

        /// <summary>
        /// Initializes a new instance of the Proxy class.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="agentCode">The agent code.</param>
        /// <param name="options">Proxy execution options.</param>
        public Proxy(HttpClient httpClient, string agentCode, ProxyExecutionReq options)
            : base(httpClient, agentCode, options)
        {
            _proxyOptions = options;
            VolatileKnowledge = new ConversationVolatileKnowledgeScope(httpClient);
        }

        /// <inheritdoc/>
        protected override object CreateExecuteBody(bool stream)
        {
            Dictionary<string, object> body = new Dictionary<string, object>();

            // Add model (required)
            body["model"] = _proxyOptions.Model;

            // Add messages (required)
            if (_proxyOptions.Messages != null && _proxyOptions.Messages.Count > 0)
            {
                body["messages"] = _proxyOptions.Messages;
            }

            // Add optional parameters
            if (_proxyOptions.Temperature.HasValue)
                body["temperature"] = _proxyOptions.Temperature.Value;

            if (_proxyOptions.MaxTokens.HasValue)
                body["max_tokens"] = _proxyOptions.MaxTokens.Value;

            if (_proxyOptions.TopP.HasValue)
                body["top_p"] = _proxyOptions.TopP.Value;

            if (_proxyOptions.TopK.HasValue)
                body["top_k"] = _proxyOptions.TopK.Value;

            if (_proxyOptions.FrequencyPenalty.HasValue)
                body["frequency_penalty"] = _proxyOptions.FrequencyPenalty.Value;

            if (_proxyOptions.PresencePenalty.HasValue)
                body["presence_penalty"] = _proxyOptions.PresencePenalty.Value;

            if (_proxyOptions.UserIdentifier != null)
                body["userIdentifier"] = _proxyOptions.UserIdentifier;

            if (_proxyOptions.Vendor != null)
                body["vendor"] = _proxyOptions.Vendor;

            if (_proxyOptions.GroupIdentifier != null)
                body["groupIdentifier"] = _proxyOptions.GroupIdentifier;

            if (_proxyOptions.UseVision.HasValue)
                body["useVision"] = _proxyOptions.UseVision.Value;

            // Add volatile knowledge IDs if any are associated
            if (VolatileKnowledge.KnowledgeIds.Any())
                body["volatileKnowledgeIds"] = VolatileKnowledge.KnowledgeIds.Select(id => id.ToString()).ToList();

            if (stream)
                body["stream"] = true;

            return body;
        }

        /// <inheritdoc/>
        protected override void OnExecutionComplete()
        {
            // Clear volatile knowledge IDs after execution
            VolatileKnowledge.ClearKnowledgeIds();
        }
    }

    /// <summary>
    /// Provides methods for interacting with AI Proxy agents.
    /// </summary>
    public sealed class AIProxiesScope
    {
        private readonly HttpClient _httpClient;

        internal AIProxiesScope(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Creates a proxy agent instance.
        /// </summary>
        /// <param name="agentCode">The agent code.</param>
        /// <param name="options">Proxy execution options.</param>
        /// <returns>A proxy instance.</returns>
        public Proxy Create(string agentCode, ProxyExecutionReq options)
            => new Proxy(_httpClient, agentCode, options);
    }
}
