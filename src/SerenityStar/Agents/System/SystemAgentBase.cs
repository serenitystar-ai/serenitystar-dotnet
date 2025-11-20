using SerenityStar.Constants;
using SerenityStar.Models.Execute;
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

namespace SerenityStar.Agents.System
{
    /// <summary>
    /// Base class for system agents.
    /// </summary>
    public abstract class SystemAgentBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _agentCode;
        /// <summary>
        /// Execution options for the agent.
        /// </summary>
        protected readonly object? Options;

        /// <summary>
        /// Initializes a new instance of the SystemAgentBase class.
        /// </summary>
        protected SystemAgentBase(HttpClient httpClient, string agentCode, object? options)
        {
            _httpClient = httpClient;
            _agentCode = agentCode;
            Options = options;
        }

        /// <summary>
        /// Creates the base parameters for execution.
        /// </summary>
        protected List<object> CreateBaseParameters(bool stream)
        {
            List<object> parameters = new List<object>();

            if (stream)
                parameters.Add(new { Key = "stream", Value = true });

            return parameters;
        }

        /// <summary>
        /// Creates the execute body. Override in derived classes.
        /// </summary>
        protected abstract object CreateExecuteBody(bool stream);

        /// <summary>
        /// Called after execution completes. Override in derived classes for cleanup.
        /// </summary>
        protected virtual void OnExecutionComplete()
        {
        }

        /// <summary>
        /// Executes the agent.
        /// </summary>
        public async Task<AgentResult> ExecuteAsync(CancellationToken cancellationToken = default)
        {
            string url = $"/api/v2/agent/{_agentCode}/execute";

            object body = CreateExecuteBody(false);

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(url, body, JsonSerializerOptionsCache.s_camelCase, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
            }

            AgentResult result = await response.Content.ReadFromJsonAsync<AgentResult>(JsonSerializerOptionsCache.s_camelCase, cancellationToken)
                   ?? throw new InvalidOperationException("Failed to deserialize response");

            OnExecutionComplete();

            return result;
        }

        /// <summary>
        /// Streams execution results.
        /// </summary>
        public async IAsyncEnumerable<StreamingAgentMessage> StreamAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            string url = $"/api/v2/agent/{_agentCode}/execute";

            object body = CreateExecuteBody(true);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(body, options: JsonSerializerOptionsCache.s_camelCase)
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

                bool executionStarted = false;
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
                        if (!executionStarted)
                        {
                            executionStarted = true;
                            OnExecutionComplete();
                        }
                        yield return msg;
                    }
                }
            }
        }
    }
}
