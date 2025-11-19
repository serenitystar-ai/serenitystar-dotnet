using System.Text.Json.Serialization;

namespace SerenityStar.Models.Connector
{
    /// <summary>
    /// Response model for connector status.
    /// </summary>
    public class ConnectorStatusResponse
    {
        /// <summary>
        /// Gets or sets a value indicating whether the connector is connected.
        /// </summary>
        [JsonPropertyName("isConnected")]
        public bool IsConnected { get; set; }
    }
}