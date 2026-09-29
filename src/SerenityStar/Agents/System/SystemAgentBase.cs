using SerenityStar.Client;
using SerenityStar.Constants;
using SerenityStar.Extensions;
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
        /// <summary>
        /// The API client used for authenticated API calls. Accessible to derived classes for file uploads.
        /// </summary>
        private protected readonly SerenityApiClient _apiClient;
        private readonly string _agentCode;
        private readonly int? _version;
        /// <summary>
        /// Execution options for the agent.
        /// </summary>
        protected readonly object? Options;

        /// <summary>
        /// Initializes a new instance of the SystemAgentBase class.
        /// </summary>
        internal SystemAgentBase(SerenityApiClient apiClient, string agentCode, object? options)
            : this(apiClient, agentCode, null, options)
        {
        }

        /// <summary>
        /// Initializes a new instance of the SystemAgentBase class with a specific version.
        /// </summary>
        internal SystemAgentBase(SerenityApiClient apiClient, string agentCode, int? version, object? options)
        {
            _apiClient = apiClient;
            _agentCode = agentCode;
            _version = version;
            Options = options;
        }

        /// <summary>
        /// Builds the execution URL, including version if specified.
        /// </summary>
        private string BuildExecuteUrl()
            => $"/api/v2/agent/{_agentCode}/execute{(_version.HasValue ? $"/{_version.Value}" : "")}";


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
        /// Called before execution to perform async preparation such as file uploads.
        /// Override in derived classes that need async setup before building the request body.
        /// </summary>
        protected virtual Task PrepareExecutionAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;

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
            await PrepareExecutionAsync(cancellationToken);

            string url = BuildExecuteUrl();

            object body = CreateExecuteBody(false);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(body, options: JsonSerializerOptionsCache.s_camelCase)
            };

            HttpResponseMessage response = await _apiClient.SendAsync(request, cancellationToken);

            AgentResult result = await response.ReadSerenityJsonAsync<AgentResult>(cancellationToken);

            OnExecutionComplete();

            return result;
        }

        /// <summary>
        /// Streams execution results.
        /// </summary>
        public async IAsyncEnumerable<StreamingAgentMessage> StreamAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await PrepareExecutionAsync(cancellationToken);

            string url = BuildExecuteUrl();

            object body = CreateExecuteBody(true);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(body, options: JsonSerializerOptionsCache.s_camelCase)
            };

            HttpResponseMessage response = await _apiClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            await response.EnsureSerenitySuccessAsync();

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
