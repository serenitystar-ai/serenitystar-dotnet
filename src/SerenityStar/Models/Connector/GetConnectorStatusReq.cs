using System;

namespace SerenityStar.Models.Connector
{
    /// <summary>
    /// Options for getting connector status.
    /// </summary>
    public sealed class GetConnectorStatusReq
    {
        /// <summary>
        /// Gets or sets the agent instance ID.
        /// </summary>
        public Guid AgentInstanceId { get; set; }
        /// <summary>
        /// Gets or sets the connector ID.
        /// </summary>
        public Guid ConnectorId { get; set; }
    }
}