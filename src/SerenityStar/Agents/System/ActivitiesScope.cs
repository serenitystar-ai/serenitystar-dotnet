using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SerenityStar.Models.Execute;

namespace SerenityStar.Agents.System
{
    /// <summary>
    /// Represents an Activity agent.
    /// </summary>
    public sealed class Activity : SystemAgentBase
    {
        internal Activity(HttpClient httpClient, string agentCode, AgentExecutionReq? options)
            : base(httpClient, agentCode, options)
        {
        }

        /// <inheritdoc/>
        protected override object CreateExecuteBody(bool stream)
        {
            List<object> parameters = CreateBaseParameters(stream);

            // Add input parameters if provided
            AgentExecutionReq? executionOptions = Options as AgentExecutionReq;
            if (executionOptions?.InputParameters != null)
            {
                foreach (KeyValuePair<string, object> param in executionOptions.InputParameters)
                {
                    parameters.Add(new { Key = param.Key, Value = param.Value });
                }
            }

            return parameters;
        }
    }

    /// <summary>
    /// Provides methods for interacting with Activity agents.
    /// </summary>
    public sealed class ActivitiesScope
    {
        private readonly HttpClient _httpClient;

        internal ActivitiesScope(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Executes an activity agent.
        /// </summary>
        /// <param name="agentCode">The agent code.</param>
        /// <param name="options">Optional execution options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task<AgentResult> ExecuteAsync(
            string agentCode,
            AgentExecutionReq? options = null,
            CancellationToken cancellationToken = default)
        {
            Activity activity = new Activity(_httpClient, agentCode, options);
            return activity.ExecuteAsync(cancellationToken);
        }

        /// <summary>
        /// Creates an activity agent for streaming.
        /// </summary>
        /// <param name="agentCode">The agent code.</param>
        /// <param name="options">Optional execution options.</param>
        /// <returns>An activity instance for streaming.</returns>
        public Activity Create(string agentCode, AgentExecutionReq? options = null)
            => new Activity(_httpClient, agentCode, options);
    }
}
