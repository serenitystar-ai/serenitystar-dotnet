using SerenityStar.Agents.VolatileKnowledge;
using SerenityStar.Client;
using SerenityStar.Models.AIProxy;
using SerenityStar.Models.Execute;
using System.Collections.Generic;
using System.Linq;

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
        /// <param name="apiClient">The API client.</param>
        /// <param name="agentCode">The agent code.</param>
        /// <param name="options">Proxy execution options.</param>
        internal Proxy(SerenityApiClient apiClient, string agentCode, ProxyExecutionReq options)
            : base(apiClient, agentCode, options)
        {
            _proxyOptions = options;
            VolatileKnowledge = new ConversationVolatileKnowledgeScope(apiClient, agentCode);
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
                body["messages"] = _proxyOptions.Messages.Select(CreateMessageBody).ToList();
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

            if (_proxyOptions.Skills != null)
                body["skills"] = CreateSkillsBody(_proxyOptions.Skills);

            // Add volatile knowledge IDs if any are associated
            if (VolatileKnowledge.KnowledgeIds.Any())
                body["volatileKnowledgeIds"] = VolatileKnowledge.KnowledgeIds.Select(id => id.ToString()).ToList();

            if (stream)
                body["stream"] = true;

            return body;
        }

        /// <summary>
        /// Builds the body of a message. The approval fields are written in snake_case, which is what the API expects
        /// for them, unlike the rest of the message.
        /// </summary>
        private static Dictionary<string, object> CreateMessageBody(ProxyExecutionMessage message)
        {
            Dictionary<string, object> body = new Dictionary<string, object>
            {
                ["role"] = message.Role,
                ["content"] = message.Content,
                ["volatileKnowledgeIds"] = message.VolatileKnowledgeIds
            };

            if (message.ToolApprovalRequest != null)
            {
                ApprovalPendingAction request = message.ToolApprovalRequest;
                body["tool_approval_request"] = new
                {
                    request_id = request.RequestId,
                    call_id = request.CallId,
                    skill_code = request.SkillCode,
                    skill_type = request.SkillType,
                    tool = request.Tool,
                    arguments = request.Arguments
                };
            }

            if (message.ToolApprovalResponse != null)
            {
                ApprovalDecision response = message.ToolApprovalResponse;
                body["tool_approval_response"] = new
                {
                    request_id = response.RequestId,
                    approved = response.Approved,
                    reason = response.Reason
                };
            }

            if (message.UserChoiceRequest != null)
                body["user_choice_request"] = new
                {
                    questions = message.UserChoiceRequest.Questions.Select(question => new
                    {
                        id = question.Id,
                        header = question.Header,
                        text = question.Text,
                        is_multiselect = question.IsMultiselect,
                        options = question.Options.Select(option => new
                        {
                            id = option.Id,
                            title = option.Title,
                            description = option.Description
                        }).ToList()
                    }).ToList()
                };

            if (message.UserChoiceResponse != null)
                body["user_choice_response"] = message.UserChoiceResponse.Select(answer => new
                {
                    question_id = answer.QuestionId,
                    selected_option_ids = answer.SelectedOptionIds,
                    other = answer.Other
                }).ToList();

            return body;
        }

        /// <summary>
        /// Builds the skills body. The API expects it in snake_case, unlike the rest of the request, and enables each
        /// skill with a flag next to its options.
        /// </summary>
        private static Dictionary<string, object> CreateSkillsBody(ProxySkillsOptions skills)
        {
            Dictionary<string, object> body = new();

            if (skills.ImageGeneration != null)
            {
                body["image_generation"] = true;
                body["image_generation_options"] = new
                {
                    model = skills.ImageGeneration.Model,
                    vendor = skills.ImageGeneration.Vendor,
                    requires_approval = skills.ImageGeneration.RequiresApproval
                };
            }

            if (skills.SpeechGeneration != null)
            {
                body["speech_generation"] = true;
                body["speech_generation_options"] = new
                {
                    model = skills.SpeechGeneration.Model,
                    vendor = skills.SpeechGeneration.Vendor,
                    voice = skills.SpeechGeneration.Voice,
                    voice_id = skills.SpeechGeneration.VoiceId,
                    speed = skills.SpeechGeneration.Speed,
                    requires_approval = skills.SpeechGeneration.RequiresApproval
                };
            }

            if (skills.WebSearch != null)
            {
                body["web_search"] = true;
                body["web_search_options"] = new
                {
                    model = skills.WebSearch.Model,
                    vendor = skills.WebSearch.Vendor,
                    requires_approval = skills.WebSearch.RequiresApproval
                };
            }

            if (skills.Workbench != null)
            {
                body["workbench"] = true;
                body["workbench_options"] = new
                {
                    requires_approval = skills.Workbench.RequiresApproval
                };
            }

            if (skills.UserChoice)
                body["user_choice"] = true;

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
        private readonly SerenityApiClient _apiClient;

        internal AIProxiesScope(SerenityApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        /// <summary>
        /// Creates a proxy agent instance.
        /// </summary>
        /// <param name="agentCode">The agent code.</param>
        /// <param name="options">Proxy execution options.</param>
        /// <returns>A proxy instance.</returns>
        public Proxy Create(string agentCode, ProxyExecutionReq options)
            => new Proxy(_apiClient, agentCode, options);
    }
}
