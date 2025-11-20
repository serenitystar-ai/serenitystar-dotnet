using SerenityStar.Models.AIProxy;
using SerenityStar.Models.Execute;
using SerenityStar.Models.Streaming;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace SerenityStar.Agents.System
{
    /// <summary>
    /// Represents a Proxy agent.
    /// </summary>
    public sealed class Proxy
    {
        private readonly HttpClient _httpClient;
        private readonly string _agentCode;
        private readonly ProxyExecutionReq _proxyOptions;
        private readonly JsonSerializerOptions _jsonOptions;

        internal Proxy(HttpClient httpClient, string agentCode, ProxyExecutionReq options)
        {
            _httpClient = httpClient;
            _agentCode = agentCode;
            _proxyOptions = options;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        /// <summary>
        /// Creates the execute body as a direct JSON object (not parameters).
        /// </summary>
        private object CreateExecuteBody(bool stream)
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

            if (stream)
                body["stream"] = true;

            return body;
        }

        /// <summary>
        /// Executes the proxy agent.
        /// </summary>
        public async Task<AgentResult> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            string url = $"/api/v2/agent/{_agentCode}/execute";
            object body = CreateExecuteBody(false);

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, body, _jsonOptions, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
            }

            return await response.Content.ReadFromJsonAsync<AgentResult>(_jsonOptions, cancellationToken)
                   ?? throw new InvalidOperationException("Failed to deserialize response");
        }

        /// <summary>
        /// Streams execution results from the proxy agent.
        /// </summary>
        public async IAsyncEnumerable<StreamingAgentMessage> StreamAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            string url = $"/api/v2/agent/{_agentCode}/execute";
            object body = CreateExecuteBody(true);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(body, options: _jsonOptions)
            };

            HttpResponseMessage response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
            }

            using Stream stream = await response.Content.ReadAsStreamAsync();
            using StreamReader reader = new(stream);
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
                    yield return msg;
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
                    "stop" => JsonSerializer.Deserialize<StreamingAgentMessageStop>(json, _jsonOptions),
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
    }

    /// <summary>
    /// Provides methods for interacting with Proxy agents.
    /// </summary>
    public sealed class ProxiesScope
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
        public Task<AgentResult> ExecuteAsync(
            string agentCode,
            ProxyExecutionReq options,
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
        public Proxy Create(string agentCode, ProxyExecutionReq options)
        {
            return new Proxy(_httpClient, agentCode, options);
        }
    }
}
