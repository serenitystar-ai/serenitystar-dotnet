using System;

namespace SerenityStar.Models.Files
{
    /// <summary>
    /// Request for uploading a file to Serenity Star from a URL.
    /// </summary>
    public sealed class UploadFileFromUrlReq
    {
        /// <summary>
        /// The URL of the file. It must be publicly accessible without authentication.
        /// </summary>
        public string FileUrl { get; set; } = null!;

        /// <summary>
        /// Optional file name including extension (e.g., "invoice.pdf").
        /// If not provided, it is taken from the URL.
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// Validates the request parameters.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when FileUrl is null, empty or whitespace.</exception>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(FileUrl))
                throw new ArgumentNullException(nameof(FileUrl), "File URL is required.");
        }
    }
}
