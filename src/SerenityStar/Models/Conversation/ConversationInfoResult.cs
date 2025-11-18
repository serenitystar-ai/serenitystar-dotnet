using System.Collections.Generic;

namespace SerenityStar.Models.Conversation
{
    /// <summary>
    /// Represents the result of getting conversation info.
    /// </summary>
    public class ConversationInfoResult
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
        public class DetailsRes
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
        public class AgentRes
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

            /// <summary>
            /// The agent's profile image ID.
            /// </summary>
            public string? ImageId { get; set; }
        }
    }
}
