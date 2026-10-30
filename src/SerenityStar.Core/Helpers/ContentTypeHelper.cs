using System.IO;

namespace SerenityStar.Helpers
{
    /// <summary>
    /// Internal helper to infer the content type of a file from its name.
    /// </summary>
    internal static class ContentTypeHelper
    {
        /// <summary>
        /// Gets the content type for the extension of <paramref name="fileName"/>. Unknown extensions are
        /// returned as application/octet-stream.
        /// </summary>
        /// <remarks>
        /// The API stores the content type sent with an upload and some services forward it as is (e.g.
        /// transcription), so it matters for files uploaded first and used later by id.
        /// </remarks>
        internal static string GetContentType(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                // Audio and video
                ".mp3" => "audio/mpeg",
                ".mp4" => "video/mp4",
                ".mpeg" => "audio/mpeg",
                ".mpga" => "audio/mpeg",
                ".m4a" => "audio/mp4",
                ".wav" => "audio/wav",
                ".webm" => "audio/webm",
                // Images
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                // Documents
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".txt" => "text/plain",
                ".csv" => "text/csv",
                ".md" => "text/markdown",
                _ => "application/octet-stream",
            };
        }
    }
}
