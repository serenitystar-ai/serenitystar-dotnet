using System;

namespace SerenityStar.Models.VolatileKnowledge
{
    /// <summary>
    /// Represents a request to upload volatile knowledge from an existing file ID.
    /// </summary>
    public sealed class UploadVolatileKnowledgeFromFileReq
    {
        /// <summary>
        /// Gets or sets the ID of the file to upload as volatile knowledge.
        /// </summary>
        public Guid FileId { get; set; }

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
            if (FileId == Guid.Empty)
                throw new ArgumentException("FileId is required.");
        }
    }
}
