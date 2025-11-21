using System;

namespace SerenityStar.Models.Execute
{
    /// <summary>
    /// Represents a pending action that requires the user to authenticate/connect to a service.
    /// </summary>
    public sealed class ConnectionPendingAction : PendingAction
    {
        /// <summary>
        /// The type of action that is pending (always "connection").
        /// </summary>
        public override string Type => "connection";

        /// <summary>
        /// The URL where the user should navigate to complete the connection.
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// The unique identifier of the connector.
        /// </summary>
        /// <remarks>
        /// The client can use it to check the connection status via the API.
        /// </remarks>
        public Guid ConnectorId { get; set; }

        /// <summary>
        /// The name of the connector/service to connect to.
        /// </summary>
        public string ConnectorName { get; set; } = string.Empty;

        /// <summary>
        /// The URL of the connector's logo/image.
        /// </summary>
        public string ConnectorImgUrl { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionPendingAction"/> class.
        /// </summary>
        public ConnectionPendingAction()
        {
        }
    }
}
