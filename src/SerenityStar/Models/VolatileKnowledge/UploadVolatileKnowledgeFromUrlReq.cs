namespace SerenityStar.Models.VolatileKnowledge
{
    /// <summary>
    /// Represents a request to upload volatile knowledge from a URL.
    /// </summary>
    public sealed class UploadVolatileKnowledgeFromUrlReq
    {
        /// <summary>
        /// Gets or sets the URL from which to download and upload the file.
        /// </summary>
        public string FileUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file name. If not provided, it will be extracted from the URL.
        /// </summary>
        public string? FileName { get; set; }

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
            if (string.IsNullOrEmpty(FileUrl))
                throw new System.ArgumentException("FileUrl is required.");
        }
    }
}
