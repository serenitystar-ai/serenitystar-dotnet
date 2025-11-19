using System;
using System.Collections.Generic;
using SerenityStar.Models.Execute;

namespace SerenityStar.Models.Streaming
{
    /// <summary>
    /// Base class for all streaming agent messages.
    /// </summary>
    public abstract class StreamingAgentMessage
    {
        /// <summary>
        /// The type of the streaming message.
        /// </summary>
        public string Type { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents the start of a streaming session.
    /// </summary>
    public class StreamingAgentMessageStart : StreamingAgentMessage
    {
        /// <summary>
        /// The UTC time when the stream started.
        /// </summary>
        public DateTime StartTimeUtc { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamingAgentMessageStart"/> class.
        /// </summary>
        public StreamingAgentMessageStart()
        {
            Type = "start";
            StartTimeUtc = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Represents the start of a task execution.
    /// </summary>
    public class StreamingAgentMessageTaskStart : StreamingAgentMessage
    {
        /// <summary>
        /// The key identifying the task.
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// The input to the task.
        /// </summary>
        public string Input { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamingAgentMessageTaskStart"/> class.
        /// </summary>
        public StreamingAgentMessageTaskStart()
        {
            Type = "task_start";
        }
    }

    /// <summary>
    /// Represents content being streamed.
    /// </summary>
    public class StreamingAgentMessageContent : StreamingAgentMessage
    {
        /// <summary>
        /// The text content.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamingAgentMessageContent"/> class.
        /// </summary>
        public StreamingAgentMessageContent()
        {
            Type = "content";
        }
    }

    /// <summary>
    /// Represents the end of a task execution.
    /// </summary>
    public class StreamingAgentMessageTaskEnd : StreamingAgentMessage
    {
        /// <summary>
        /// The key identifying the task.
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// The result of the task execution.
        /// </summary>
        public object? Result { get; set; }

        /// <summary>
        /// The duration of the task in milliseconds.
        /// </summary>
        public long DurationMs { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamingAgentMessageTaskEnd"/> class.
        /// </summary>
        public StreamingAgentMessageTaskEnd()
        {
            Type = "task_end";
        }
    }

    /// <summary>
    /// Represents the completion of a streaming session.
    /// </summary>
    public class StreamingAgentMessageStop : StreamingAgentMessage
    {
        /// <summary>
        /// The complete content.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// The instance ID of the agent.
        /// </summary>
        public Guid? InstanceId { get; set; }

        /// <summary>
        /// Token usage information.
        /// </summary>
        public AgentResultTokenUsage? CompletionUsage { get; set; }

        /// <summary>
        /// Time to first token in milliseconds.
        /// </summary>
        public long? TimeToFirstToken { get; set; }

        /// <summary>
        /// Executor task logs.
        /// </summary>
        public List<ExecutorTaskResult>? ExecutorTaskLogs { get; set; }

        /// <summary>
        /// Action results from the agent.
        /// </summary>
        public Dictionary<string, object>? ActionResults { get; set; }

        /// <summary>
        /// The agent message ID.
        /// </summary>
        public string? AgentMessageId { get; set; }

        /// <summary>
        /// The user message ID.
        /// </summary>
        public string? UserMessageId { get; set; }

        /// <summary>
        /// The complete agent result with all execution details.
        /// </summary>
        public AgentResult? Result { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamingAgentMessageStop"/> class.
        /// </summary>
        public StreamingAgentMessageStop()
        {
            Type = "stop";
        }
    }

    /// <summary>
    /// Represents an error during streaming.
    /// </summary>
    public class StreamingAgentMessageError : StreamingAgentMessage
    {
        /// <summary>
        /// The error message.
        /// </summary>
        public string Error { get; set; } = string.Empty;

        /// <summary>
        /// The HTTP status code, if applicable.
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamingAgentMessageError"/> class.
        /// </summary>
        public StreamingAgentMessageError()
        {
            Type = "error";
        }
    }
}
