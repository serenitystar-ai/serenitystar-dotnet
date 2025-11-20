namespace SerenityStar.Models.Connector
{
    /// <summary>
    /// Result of connector status check.
    /// </summary>
    public sealed class ConnectorStatusResult
    {
        /// <summary>
        /// Indicates if the connector is connected.
        /// </summary>
        public bool IsConnected { get; set; }
    }
}
