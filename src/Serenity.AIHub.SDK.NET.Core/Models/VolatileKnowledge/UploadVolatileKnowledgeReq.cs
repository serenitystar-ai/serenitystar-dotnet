namespace Serenity.AIHub.SDK.NET.Core.Models.VolatileKnowledge;

/// <summary>
/// Represents a request to upload volatile knowledge.
/// Can contain either file content or text content, but not both.
/// </summary>
public class UploadVolatileKnowledgeReq
{
    /// <summary>
    /// Gets or sets the text content for volatile knowledge.
    /// Either Content or FileStream must be provided, but not both.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the callback URL to be notified when processing is complete.
    /// </summary>
    public string CallbackUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file stream to upload.
    /// Either Content or FileStream must be provided, but not both.
    /// </summary>
    public Stream FileStream { get; set; }

    /// <summary>
    /// Gets or sets the file name.
    /// Required if FileStream is provided.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Validates that either Content or FileStream is provided, but not both.
    /// </summary>
    public void Validate()
    {
        bool hasContent = !string.IsNullOrEmpty(Content);
        bool hasFile = FileStream is not null;

        if (hasContent && hasFile)
            throw new ArgumentException("Only one of Content or FileStream should be provided, not both.");

        if (!hasContent && !hasFile)
            throw new ArgumentException("Either Content or FileStream must be provided.");

        if (hasFile && string.IsNullOrEmpty(FileName))
            throw new ArgumentException("FileName is required when FileStream is provided.");
    }
}
