namespace SerenityStar.Models.Conversation
{
    /// <summary>
    /// Options for checking connector status.
    /// </summary>
    public class GetConnectorStatusOptions
    {
        /// <summary>
        /// The agent instance ID.
        /// </summary>
        public string AgentInstanceId { get; set; } = string.Empty;

        /// <summary>
        /// The connector ID.
        /// </summary>
        public string ConnectorId { get; set; } = string.Empty;
    }
}
