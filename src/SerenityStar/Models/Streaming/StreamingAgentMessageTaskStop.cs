using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using SerenityStar.Models.Execute;

namespace SerenityStar.Models.Streaming
{
    /// <summary>
    /// Represents the end of a task execution.
    /// </summary>
    public sealed class StreamingAgentMessageTaskStop : StreamingAgentMessage
    {
        /// <inheritdoc />
        public override string Type => "task_stop";

        /// <summary>
        /// The name of the task.
        /// </summary>
        public string Task { get; set; } = string.Empty;

        /// <summary>
        /// The key identifying the task.
        /// </summary>
        public string TaskKey { get; set; } = string.Empty;

        /// <summary>
        /// The UTC time when the task started.
        /// </summary>
        public DateTime StartTimeUtc { get; set; }

        /// <summary>
        /// The UTC time when the task ended.
        /// </summary>
        public DateTime EndTimeUtc { get; set; }

        /// <summary>
        /// The duration of the task execution.
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// The output of the task execution.
        /// </summary>
        public object? Output { get; set; }

        /// <summary>
        /// Whether the task completed successfully.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets the skill approval the agent requested when this task is an approval request
        /// (<see cref="StreamingTaskKeys.ApprovalRequest"/>).
        /// </summary>
        /// <remarks>
        /// It lets the approval be shown as soon as it is requested. The same approvals are also listed in the
        /// <see cref="StreamingAgentMessageStop.Result"/> pending actions once the stream ends.
        /// </remarks>
        /// <param name="approval">The requested approval, or null when this task is not an approval request.</param>
        /// <returns>True when this task is an approval request.</returns>
        public bool TryGetApprovalRequest([NotNullWhen(true)] out ApprovalPendingAction? approval)
        {
            approval = null;

            if (TaskKey != StreamingTaskKeys.ApprovalRequest || !(Output is JsonElement { ValueKind: JsonValueKind.Object } output))
                return false;

            approval = output.Deserialize<ApprovalPendingAction>(StreamingAgentMessageJsonConverter.s_options);
            return approval != null;
        }

        /// <summary>
        /// Gets the questions the agent asked when this task is a user choice request
        /// (<see cref="StreamingTaskKeys.UserChoiceRequest"/>).
        /// </summary>
        /// <remarks>
        /// It lets the questions be shown as soon as they are asked. The same request is also listed in the
        /// <see cref="StreamingAgentMessageStop.Result"/> pending actions once the stream ends.
        /// </remarks>
        /// <param name="userChoice">The requested user choice, or null when this task is not a user choice request.</param>
        /// <returns>True when this task is a user choice request.</returns>
        public bool TryGetUserChoiceRequest([NotNullWhen(true)] out UserChoicePendingAction? userChoice)
        {
            userChoice = null;

            if (TaskKey != StreamingTaskKeys.UserChoiceRequest || !(Output is JsonElement { ValueKind: JsonValueKind.Object } output))
                return false;

            userChoice = output.Deserialize<UserChoicePendingAction>(StreamingAgentMessageJsonConverter.s_options);
            return userChoice != null;
        }

        /// <summary>
        /// Gets the agent's TODO list when this task reports it: an Agent TODOs skill invocation (adding,
        /// completing, canceling or reading items) or a new iteration of the agent (<see cref="StreamingTaskKeys.AgentTodoIterate"/>).
        /// </summary>
        /// <remarks>
        /// Each of these tasks carries the whole list, so the latest one received holds the current progress.
        /// </remarks>
        /// <param name="todos">The current TODO list, or null when this task does not report it.</param>
        /// <returns>True when this task reports the TODO list.</returns>
        public bool TryGetAgentTodos([NotNullWhen(true)] out IReadOnlyList<AgentTodoItem>? todos)
        {
            todos = null;

            if (!(Output is JsonElement output))
                return false;

            // The iteration task carries the bare list; skill tasks wrap it in a result whose type is the skill type.
            JsonElement list;
            if (TaskKey == StreamingTaskKeys.AgentTodoIterate)
                list = output;
            else if (output.ValueKind != JsonValueKind.Object
                || !output.TryGetProperty("type", out JsonElement type)
                || type.ValueKind != JsonValueKind.String
                || type.GetString() != AgentTodoSkillType
                || !output.TryGetProperty("content", out list))
                return false;

            if (list.ValueKind != JsonValueKind.Array)
                return false;

            todos = list.Deserialize<List<AgentTodoItem>>(StreamingAgentMessageJsonConverter.s_options);
            return todos != null;
        }

        private const string AgentTodoSkillType = "AgentTodo";
    }
}