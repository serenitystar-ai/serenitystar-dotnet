using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SerenityStar.Models.Execute;
using SerenityStar.Models.Streaming;

namespace SerenityStar.Agents.System
{
    /// <summary>
    /// Base class for system agents.
    /// </summary>
    public abstract class SystemAgentBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _agentCode;
        protected readonly object? Options;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Initializes a new instance of the SystemAgentBase class.
        /// </summary>
        protected SystemAgentBase(HttpClient httpClient, string agentCode, object? options)
        {
            _httpClient = httpClient;
            _agentCode = agentCode;
            Options = options;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        /// <summary>
        /// Creates the base parameters for execution.
        /// </summary>
        protected List<object> CreateBaseParameters(bool stream)
        {
            List<object> parameters = new List<object>();

            if (stream)
            {
                parameters.Add(new { Key = "stream", Value = true });
            }

            return parameters;
        }

        /// <summary>
        /// Creates the execute body. Override in derived classes.
        /// </summary>
        protected abstract object CreateExecuteBody(bool stream);

        /// <summary>
        /// Executes the agent.
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
        /// Streams execution results.
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
                        yield return msg;
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
}
