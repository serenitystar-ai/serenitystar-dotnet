using SerenityStar.Agents.VolatileKnowledge;
using SerenityStar.Constants;
using SerenityStar.Helpers;
using SerenityStar.Models.Execute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SerenityStar.Agents.System
{
    /// <summary>
    /// Represents an Activity agent.
    /// </summary>
    public sealed class Activity : SystemAgentBase
    {
        private Guid? _uploadedAudioFileId;

        /// <summary>
        /// Provides methods for managing volatile knowledge within this activity.
        /// Uploaded knowledge is automatically associated with this activity.
        /// </summary>
        public ConversationVolatileKnowledgeScope VolatileKnowledge { get; }

        /// <summary>
        /// Initializes a new instance of the Activity class.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="agentCode">The agent code.</param>
        /// <param name="version">Optional specific version of the agent to execute. If not specified, uses the published version.</param>
        /// <param name="options">Optional execution options.</param>
        public Activity(HttpClient httpClient, string agentCode, int? version = null, AgentExecutionReq? options = null)
            : base(httpClient, agentCode, version, options)
        {
            VolatileKnowledge = new ConversationVolatileKnowledgeScope(httpClient);
        }

        /// <inheritdoc/>
        protected override async Task PrepareExecutionAsync(CancellationToken cancellationToken = default)
        {
            AgentExecutionReq? executionOptions = Options as AgentExecutionReq;
            if (executionOptions?.AudioFileStream != null)
            {
                if (string.IsNullOrEmpty(executionOptions.AudioFileName))
                    throw new ArgumentNullException(nameof(executionOptions.AudioFileName), "Audio file name is required when providing an audio file stream.");

                _uploadedAudioFileId = await FileUploadHelper.UploadFileAsync(
                    _httpClient, executionOptions.AudioFileStream, executionOptions.AudioFileName, cancellationToken);
            }
        }

        /// <inheritdoc/>
        protected override object CreateExecuteBody(bool stream)
        {
            List<object> parameters = CreateBaseParameters(stream);

            // Add input parameters if provided
            AgentExecutionReq? executionOptions = Options as AgentExecutionReq;
            if (executionOptions?.InputParameters != null)
                foreach (KeyValuePair<string, object> param in executionOptions.InputParameters)
                    parameters.Add(new { param.Key, param.Value });

            // Add audio input if file was uploaded
            if (_uploadedAudioFileId.HasValue)
                parameters.Add(new { Key = "audioInput", Value = JsonSerializer.Serialize(new { fileId = _uploadedAudioFileId.Value }, JsonSerializerOptionsCache.s_camelCase) });

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
        /// Creates an activity agent instance.
        /// </summary>
        /// <param name="agentCode">The agent code.</param>
        /// <param name="options">Optional execution options.</param>
        /// <returns>An activity instance.</returns>
        public Activity Create(string agentCode, AgentExecutionReq? options = null)
            => new Activity(_httpClient, agentCode, null, options);

        /// <summary>
        /// Creates an activity agent instance with a specific version.
        /// </summary>
        /// <param name="agentCode">The agent code.</param>
        /// <param name="version">The specific version of the agent to execute.</param>
        /// <param name="options">Optional execution options.</param>
        /// <returns>An activity instance.</returns>
        public Activity Create(string agentCode, int version, AgentExecutionReq? options = null)
            => new Activity(_httpClient, agentCode, version, options);
    }
}
