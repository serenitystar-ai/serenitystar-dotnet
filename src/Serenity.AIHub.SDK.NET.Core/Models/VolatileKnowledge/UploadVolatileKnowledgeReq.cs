namespace Serenity.AIHub.SDK.NET.Core.Models.VolatileKnowledge;

public class UploadVolatileKnowledgeReq
{
    public Stream FileStream { get; set; } = Stream.Null;
    public string FileName { get; set; } = string.Empty;
}
