using SerenityStar.Errors.Models;

namespace SerenityStar.Errors.Constants
{
    /// <summary>
    /// The keys of <see cref="ValidationError.Errors"/> on the execution and conversation endpoints.
    /// Each key identifies the business rule that failed.
    /// </summary>
    public static class ValidationErrorKeys
    {
        #region Request input

        /// <summary>
        /// No message was supplied for the agent.
        /// </summary>
        public const string MessageRequired = "message_required";

        /// <summary>
        /// The request body is structurally invalid for this endpoint.
        /// </summary>
        public const string InvalidRequestBody = "invalid_request_body";

        /// <summary>
        /// The same input key appears more than once.
        /// </summary>
        public const string InputKeysDuplicated = "input_keys_duplicated";

        /// <summary>
        /// More than one input was provided where a single one is required.
        /// </summary>
        public const string MultipleInputs = "multiple_inputs";

        /// <summary>
        /// One or more required input parameters were not provided.
        /// </summary>
        public const string RequiredParametersMissing = "required_parameters_missing";

        /// <summary>
        /// A required parameter was provided as null.
        /// </summary>
        public const string RequiredParametersNull = "required_parameters_null";

        /// <summary>
        /// A required parameter was provided but empty.
        /// </summary>
        public const string RequiredParametersEmpty = "required_parameters_empty";

        /// <summary>
        /// A parameter value does not match its expected type.
        /// </summary>
        public const string ParameterTypeMismatch = "parameter_type_mismatch";

        /// <summary>
        /// The request omits variables the agent expects.
        /// </summary>
        public const string MissingVariables = "missing_variables";

        /// <summary>
        /// The supplied message list is invalid.
        /// </summary>
        public const string InvalidMessages = "invalid_messages";

        /// <summary>
        /// Audio input was sent to an agent or model that does not accept it.
        /// </summary>
        public const string AudioInputNotSupported = "audio_input_not_supported";

        #endregion

        #region Agent state

        /// <summary>
        /// The target agent is not active.
        /// </summary>
        public const string AgentInactive = "agent_inactive";

        /// <summary>
        /// The agent code does not resolve to an agent.
        /// </summary>
        public const string AgentCodeInvalid = "agent_code_invalid";

        /// <summary>
        /// The requested agent version is not active.
        /// </summary>
        public const string AgentVersionInactive = "agent_version_inactive";

        /// <summary>
        /// The selected model is not permitted for this agent.
        /// </summary>
        public const string AIModelNotAllowed = "ai_model_not_allowed";

        /// <summary>
        /// The requested model is invalid.
        /// </summary>
        public const string InvalidModel = "invalid_model";

        #endregion

        #region Conversation

        /// <summary>
        /// The conversation is closed and cannot accept new messages.
        /// </summary>
        public const string ConversationClosed = "conversation_closed";

        /// <summary>
        /// The referenced conversation context does not exist.
        /// </summary>
        public const string ConversationContextNotFound = "conversation_context_not_found";

        #endregion

        #region Response format and reasoning options

        /// <summary>
        /// The requested response-format type is invalid.
        /// </summary>
        public const string InvalidResponseFormatType = "invalid_response_format_type";

        /// <summary>
        /// The supplied response-format schema is invalid.
        /// </summary>
        public const string InvalidResponseFormatSchema = "invalid_response_format_schema";

        /// <summary>
        /// The model does not support the requested response format.
        /// </summary>
        public const string ResponseFormatNotSupportedByModel = "response_format_not_supported_by_model";

        /// <summary>
        /// The requested reasoning-effort value is invalid.
        /// </summary>
        public const string InvalidReasoningEffort = "invalid_reasoning_effort";

        /// <summary>
        /// The requested reasoning-detail value is invalid.
        /// </summary>
        public const string InvalidReasoningDetail = "invalid_reasoning_detail";

        /// <summary>
        /// The model does not support a reasoning-effort setting.
        /// </summary>
        public const string ReasoningEffortNotSupportedByModel = "reasoning_effort_not_supported_by_model";

        /// <summary>
        /// The model does not support a reasoning-detail setting.
        /// </summary>
        public const string ReasoningDetailNotSupportedByModel = "reasoning_detail_not_supported_by_model";

        #endregion

        #region Skills and tools

        /// <summary>
        /// The supplied skills options are invalid.
        /// </summary>
        public const string InvalidSkillsOptions = "invalid_skills_options";

        /// <summary>
        /// The supplied skills options conflict with each other.
        /// </summary>
        public const string ConflictingSkillsOptions = "conflicting_skills_options";

        /// <summary>
        /// The run is paused waiting for a tool call to be approved.
        /// </summary>
        public const string ToolApprovalPending = "tool_approval_pending";

        /// <summary>
        /// The skill referenced by a tool approval does not exist.
        /// </summary>
        public const string ToolApprovalSkillNotFound = "tool_approval_skill_not_found";

        /// <summary>
        /// A tool approval matched more than one tool.
        /// </summary>
        public const string ToolApprovalAmbiguousTool = "tool_approval_ambiguous_tool";

        #endregion

        #region Quota and balance

        /// <summary>
        /// The account balance is insufficient to run the request.
        /// </summary>
        public const string InsufficientBalance = "insufficient_balance";

        /// <summary>
        /// The monthly usage quota is exhausted.
        /// </summary>
        public const string MonthlyQuotaExceeded = "monthly_quota_exceeded";

        /// <summary>
        /// The per-user usage quota is exhausted.
        /// </summary>
        public const string UserQuotaExceeded = "user_quota_exceeded";

        /// <summary>
        /// The organisation-wide usage quota is exhausted.
        /// </summary>
        public const string OrganisationQuotaExceeded = "organisation_quota_exceeded";

        /// <summary>
        /// The quota for the specific model is exhausted.
        /// </summary>
        public const string ModelQuotaExceeded = "model_quota_exceeded";

        /// <summary>
        /// The balance is insufficient for a non-bonified execution.
        /// </summary>
        public const string ExcludedBonifiedExecutionInsufficientBalance = "excluded_bonified_execution_insufficient_balance";

        #endregion
    }
}
