using System;

namespace SerenityStar.Models.VolatileKnowledge
{
    /// <summary>
    /// Represents a request to upload volatile knowledge from a base64-encoded file.
    /// </summary>
    public sealed class UploadVolatileKnowledgeFromBase64Req
    {
        /// <summary>
        /// Gets or sets the name of the file being uploaded, including its extension.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the MIME type of the file (e.g. "application/pdf", "text/plain").
        /// </summary>
        public string MimeType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the base64-encoded content of the file.
        /// </summary>
        public string ContentBase64 { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the callback URL to be notified when processing is complete.
        /// </summary>
        public string? CallbackUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the knowledge should not expire. Default is false.
        /// </summary>
        public bool NoExpiration { get; set; }

        /// <summary>
        /// Gets or sets the number of days until expiration.
        /// If not provided, the default expiration days from system configuration will be used.
        /// </summary>
        public int? ExpirationDays { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to process embeddings. Default is false.
        /// </summary>
        public bool ProcessEmbeddings { get; set; }

        /// <summary>
        /// Validates that the required values are provided.
        /// </summary>
        public void Validate()
        {
            if (string.IsNullOrEmpty(FileName))
                throw new ArgumentException("FileName is required.");

            if (string.IsNullOrEmpty(MimeType))
                throw new ArgumentException("MimeType is required.");

            if (string.IsNullOrEmpty(ContentBase64))
                throw new ArgumentException("ContentBase64 is required.");
        }
    }
}
