using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SerenityStar.Models.Execute
{
    /// <summary>
    /// Represents the result of an agent execution.
    /// </summary>
    public sealed class AgentResult
    {
        /// <summary>
        /// The response from the agent.
        /// Typically, an AI generate answer.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// If the content is a JSON object, the deserialized object representation of it.
        /// </summary>
        public object? JsonContent { get; set; }

        /// <summary>
        /// The instance id of the agent.
        /// </summary>
        public Guid InstanceId { get; set; }

        /// <summary>
        /// Representation of the token counts processed for a completions request.
        /// </summary>
        public TokenUsage? CompletionUsage { get; set; }

        /// <summary>
        /// The cost information for this agent execution in the tenant's currency.
        /// </summary>
        public CostInfo? Cost { get; set; }

        /// <summary>
        /// The results of the actions performed by the Agent, if any.
        /// Such as the response from Plugins executed or other Agents invoked.
        /// </summary>
        public Dictionary<string, object>? ActionResults { get; set; }

        /// <summary>
        /// Representation of the MetaAnalysis.
        /// </summary>
        public object? MetaAnalysis { get; set; }

        /// <summary>
        /// The list of guardrail rule violations detected during the agent execution.
        /// </summary>
        public List<GuardrailRuleViolation>? Violations { get; set; }

        /// <summary>
        /// The time taken to generate the first token.
        /// </summary>
        public long? TimeToFirstToken { get; set; }

        /// <summary>
        /// The tasks executed to complete the request and their duration in milliseconds.
        /// </summary>
        public List<Task>? ExecutorTaskLogs { get; set; }

        /// <summary>
        /// The id of the user message.
        /// </summary>
        public Guid? UserMessageId { get; set; }

        /// <summary>
        /// The id of the agent message.
        /// </summary>
        public Guid? AgentMessageId { get; set; }

        /// <summary>
        /// Actions that need to be completed by the user to unlock agent features.
        /// For example, authentication to external services.
        /// </summary>
        public List<PendingAction> PendingActions { get; set; } = new List<PendingAction>();

        /// <summary>
        /// Collection of sensitive data items that were detected and obfuscated during this request.
        /// </summary>
        public List<SensitiveDataItem> SensitiveData { get; set; } = new List<SensitiveDataItem>();

        /// <summary>
        /// Additional metadata from the execution.
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentResult"/> class with default values.
        /// </summary>
        public AgentResult() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentResult"/> class with the specified content and optional action results,
        /// attempting to deserialize the content into <see cref="JsonContent"/> if it is valid JSON.
        /// </summary>
        [JsonConstructor]
        public AgentResult(string content, Dictionary<string, object>? actionResults = null)
        {
            Content = content;
            ActionResults = actionResults;
            JsonContent = TryParseJson(content);
        }

        private static object? TryParseJson(string jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
                return null;

            try
            {
                return JsonSerializer.Deserialize<dynamic>(jsonString, JsonSerializerOptions.Default);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>
        /// Represents token usage statistics for an agent execution.
        /// </summary>
        public sealed class TokenUsage
        {
            /// <summary>
            /// Number of tokens in the prompt.
            /// </summary>
            public int PromptTokens { get; set; }

            /// <summary>
            /// Number of tokens in the completion.
            /// </summary>
            public int CompletionTokens { get; set; }

            /// <summary>
            /// Total number of tokens used.
            /// </summary>
            public int TotalTokens { get; set; }
        }

        /// <summary>
        /// Represents cost information for an agent execution.
        /// </summary>
        public sealed class CostInfo
        {
            /// <summary>
            /// The total cost of the execution.
            /// </summary>
            public decimal Total { get; set; }

            /// <summary>
            /// The currency code (e.g., USD, EUR).
            /// </summary>
            public string Currency { get; set; } = string.Empty;
        }

        /// <summary>
        /// Represents the result of an executor task.
        /// </summary>
        public sealed class Task
        {
            /// <summary>
            /// The unique key identifier of the task.
            /// </summary>
            public string Key { get; set; } = string.Empty;

            /// <summary>
            /// The description or display name of the task.
            /// </summary>
            public string Description { get; set; } = string.Empty;

            /// <summary>
            /// The duration of the task in milliseconds.
            /// </summary>
            public long Duration { get; set; }

            /// <summary>
            /// Indicates whether the task completed successfully.
            /// </summary>
            public bool Success { get; set; }
        }

        /// <summary>
        /// Represents a guardrail rule violation detected during agent execution.
        /// </summary>
        public sealed class GuardrailRuleViolation
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="GuardrailRuleViolation"/> class.
            /// </summary>
            public GuardrailRuleViolation()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="GuardrailRuleViolation"/> class with rule and score.
            /// </summary>
            public GuardrailRuleViolation(string rule, double? score)
            {
                Rule = rule;
                ScoreInput = score;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="GuardrailRuleViolation"/> class with full details.
            /// </summary>
            public GuardrailRuleViolation(string rule, bool isInputScore, double? score, string? ruleDisplayName = null)
            {
                Rule = rule;
                RuleName = rule;
                RuleDisplayName = ruleDisplayName ?? rule;
                ScoreInput = isInputScore ? score : null;
                ScoreOutput = !isInputScore ? score : null;
            }

            /// <summary>
            /// The guardrail rule that was violated.
            /// </summary>
            public string Rule { get; set; } = string.Empty;

            /// <summary>
            /// The name of the rule.
            /// </summary>
            public string? RuleName { get; set; }

            /// <summary>
            /// The score for input violations, if applicable.
            /// </summary>
            public double? ScoreInput { get; set; }

            /// <summary>
            /// The score for output violations, if applicable.
            /// </summary>
            public double? ScoreOutput { get; set; }

            /// <summary>
            /// The display name of the rule.
            /// </summary>
            public string? RuleDisplayName { get; set; }
        }

        /// <summary>
        /// Represents a single sensitive data item that was obfuscated.
        /// </summary>
        public sealed class SensitiveDataItem
        {
            /// <summary>
            /// The original content before obfuscation.
            /// </summary>
            public string OriginalContent { get; set; } = string.Empty;

            /// <summary>
            /// The obfuscated content (e.g., "PERSON-1", "EMAIL_ADDRESS-1").
            /// </summary>
            public string ObfuscatedContent { get; set; } = string.Empty;

            /// <summary>
            /// The confidence score of the detection (0.0 to 1.0).
            /// </summary>
            public double Score { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="SensitiveDataItem"/> class.
            /// </summary>
            public SensitiveDataItem()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="SensitiveDataItem"/> class with specified values.
            /// </summary>
            public SensitiveDataItem(string originalContent, string obfuscatedContent, double score)
            {
                OriginalContent = originalContent;
                ObfuscatedContent = obfuscatedContent;
                Score = score;
            }
        }
    }
}