using System.Collections.Generic;

namespace SerenityStar.Models.Conversation
{
    /// <summary>
    /// Represents the result of getting conversation info.
    /// </summary>
    public sealed class ConversationInfoResult
    {
        /// <summary>
        /// Conversation information.
        /// </summary>
        public DetailsRes Conversation { get; set; } = new DetailsRes();

        /// <summary>
        /// Agent information.
        /// </summary>
        public AgentRes Agent { get; set; } = new AgentRes();

        /// <summary>
        /// Chat widget configuration.
        /// </summary>
        public SerenityChatResult? Channel { get; set; }

        /// <summary>
        /// Represents conversation details.
        /// </summary>
        public sealed class DetailsRes
        {
            /// <summary>
            /// The initial message for the conversation.
            /// </summary>
            public string InitialMessage { get; set; } = string.Empty;

            /// <summary>
            /// Suggested conversation starters.
            /// </summary>
            public List<string> Starters { get; set; } = new List<string>();
        }

        /// <summary>
        /// Represents agent information.
        /// </summary>
        public sealed class AgentRes
        {
            /// <summary>
            /// The version of the agent.
            /// </summary>
            public int Version { get; set; }

            /// <summary>
            /// Indicates if vision is enabled.
            /// </summary>
            public bool VisionEnabled { get; set; }

            /// <summary>
            /// Indicates if the agent supports realtime.
            /// </summary>
            public bool IsRealtime { get; set; }
        }
    }
}
