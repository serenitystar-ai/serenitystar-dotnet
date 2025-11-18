using System.Text.Json.Serialization;

namespace SerenityStar.Models.Connector;

public class ConnectorStatusResponse
{
    [JsonPropertyName("isConnected")]
    public bool IsConnected { get; set; }
}
