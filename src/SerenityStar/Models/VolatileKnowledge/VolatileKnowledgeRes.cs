using System;

namespace SerenityStar.Models.VolatileKnowledge
{

    /// <summary>
    /// Represents volatile knowledge entity with processing status and expiration tracking.
    /// </summary>
    public sealed class VolatileKnowledgeRes
    {
        /// <summary>
        /// Gets or sets the unique identifier of the volatile knowledge.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the expiration date in UTC.
        /// </summary>
        public DateTime? ExpirationDate { get; set; }

        /// <summary>
        /// Gets or sets the status of the volatile knowledge processing.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        public string Filename { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        public long? FileSize { get; set; }

        /// <summary>
        /// Gets or sets the file ID associated with the uploaded file.
        /// </summary>
        public Guid? FileId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether embeddings have been generated for this knowledge.
        /// </summary>
        public bool EmbeddingsGenerated { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the content has been scanned.
        /// </summary>
        public bool Scanned { get; set; }
    }
}