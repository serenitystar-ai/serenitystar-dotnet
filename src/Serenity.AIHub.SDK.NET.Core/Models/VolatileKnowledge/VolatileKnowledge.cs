using System.Text.Json.Serialization;

namespace Serenity.AIHub.SDK.NET.Core.Models.VolatileKnowledge;

/// <summary>
/// Represents volatile knowledge entity with processing status and expiration tracking.
/// </summary>
public class VolatileKnowledge
{
    /// <summary>
    /// Gets or sets the unique identifier of the volatile knowledge.
    /// </summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the status of the volatile knowledge processing.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = VolatileKnowledgeSimpleStatus.Analyzing;

    /// <summary>
    /// Gets or sets the file name.
    /// </summary>
    [JsonPropertyName("filename")]
    public string Filename { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file ID associated with the uploaded file.
    /// </summary>
    [JsonPropertyName("fileId")]
    public Guid? FileId { get; set; }

    /// <summary>
    /// Gets or sets the creation timestamp.
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    [JsonPropertyName("updatedAt")]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the expiration date in UTC.
    /// </summary>
    [JsonPropertyName("expirationDate")]
    public DateTime? ExpirationDate { get; set; }

    /// <summary>
    /// Gets or sets the error message if processing failed.
    /// </summary>
    [JsonPropertyName("error")]
    public string Error { get; set; } = string.Empty;
}
