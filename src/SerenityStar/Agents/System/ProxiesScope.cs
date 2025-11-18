using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SerenityStar.Models.AIProxy;

namespace SerenityStar.Agents.System
{
    /// <summary>
    /// Represents a Proxy agent.
    /// </summary>
    public class Proxy : SystemAgentBase
    {
        private readonly ProxyExecutionOptions _proxyOptions;

        internal Proxy(HttpClient httpClient, string agentCode, ProxyExecutionOptions options)
            : base(httpClient, agentCode, options)
        {
            _proxyOptions = options;
        }

        /// <inheritdoc/>
        protected override object CreateExecuteBody(bool stream)
        {
            List<object> parameters = CreateBaseParameters(stream);

            // Add proxy-specific parameters
            parameters.Add(new { Key = "model", Value = _proxyOptions.Model });

            if (_proxyOptions.Messages != null && _proxyOptions.Messages.Count > 0)
            {
                parameters.Add(new { Key = "messages", Value = JsonSerializer.Serialize(_proxyOptions.Messages) });
            }

            if (_proxyOptions.Temperature.HasValue)
            {
                parameters.Add(new { Key = "temperature", Value = _proxyOptions.Temperature.Value });
            }

            if (_proxyOptions.MaxTokens.HasValue)
            {
                parameters.Add(new { Key = "max_tokens", Value = _proxyOptions.MaxTokens.Value });
            }

            if (_proxyOptions.TopP.HasValue)
            {
                parameters.Add(new { Key = "top_p", Value = _proxyOptions.TopP.Value });
            }

            if (_proxyOptions.TopK.HasValue)
            {
                parameters.Add(new { Key = "top_k", Value = _proxyOptions.TopK.Value });
            }

            if (_proxyOptions.FrequencyPenalty.HasValue)
            {
                parameters.Add(new { Key = "frequency_penalty", Value = _proxyOptions.FrequencyPenalty.Value });
            }

            if (_proxyOptions.PresencePenalty.HasValue)
            {
                parameters.Add(new { Key = "presence_penalty", Value = _proxyOptions.PresencePenalty.Value });
            }

            if (_proxyOptions.Vendor != null)
            {
                parameters.Add(new { Key = "vendor", Value = _proxyOptions.Vendor });
            }

            if (_proxyOptions.GroupIdentifier != null)
            {
                parameters.Add(new { Key = "groupIdentifier", Value = _proxyOptions.GroupIdentifier });
            }

            if (_proxyOptions.UseVision.HasValue)
            {
                parameters.Add(new { Key = "useVision", Value = _proxyOptions.UseVision.Value });
            }

            return parameters;
        }
    }

    /// <summary>
    /// Provides methods for interacting with Proxy agents.
    /// </summary>
    public class ProxiesScope
    {
        private readonly HttpClient _httpClient;

        internal ProxiesScope(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Executes a proxy agent.
        /// </summary>
        /// <param name="agentCode">The agent code.</param>
        /// <param name="options">Proxy execution options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task<Models.Execute.AgentResult> ExecuteAsync(
            string agentCode,
            ProxyExecutionOptions options,
            CancellationToken cancellationToken = default)
        {
            Proxy proxy = new Proxy(_httpClient, agentCode, options);
            return proxy.ExecuteAsync(cancellationToken);
        }

        /// <summary>
        /// Creates a proxy agent for streaming.
        /// </summary>
        /// <param name="agentCode">The agent code.</param>
        /// <param name="options">Proxy execution options.</param>
        /// <returns>A proxy instance for streaming.</returns>
        public Proxy Create(string agentCode, ProxyExecutionOptions options)
        {
            return new Proxy(_httpClient, agentCode, options);
        }
    }
}
