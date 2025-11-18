using System.Text.Json.Serialization;

namespace Serenity.AIHub.SDK.NET.Core.Models.VolatileKnowledge;

public class VolatileKnowledge
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("filename")]
    public string Filename { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    [JsonPropertyName("error")]
    public string Error { get; set; }
}
