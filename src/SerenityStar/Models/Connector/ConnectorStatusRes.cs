namespace SerenityStar.Models.Connector
{
    /// <summary>
    /// Response model for connector status.
    /// </summary>
    public sealed class ConnectorStatusRes
    {
        /// <summary>
        /// Gets or sets a value indicating whether the connector is connected.
        /// </summary>
        public bool IsConnected { get; set; }
    }
}