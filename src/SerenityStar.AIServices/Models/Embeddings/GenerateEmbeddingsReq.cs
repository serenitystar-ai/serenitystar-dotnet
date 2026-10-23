using System;
using System.Collections.Generic;

namespace SerenityStar.Models.Embeddings
{
    /// <summary>
    /// Request for generating embeddings for one or more texts.
    /// </summary>
    public sealed class GenerateEmbeddingsReq
    {
        /// <summary>
        /// The AI Model Id to use for embeddings.
        /// If not provided, the default embedding model will be used.
        /// </summary>
        public Guid? ModelId { get; set; }

        /// <summary>
        /// The texts to embed. At least one is required.
        /// </summary>
        public List<string> Content { get; set; } = new List<string>();

        /// <summary>
        /// Optional user identifier for tracking and cost management purposes.
        /// </summary>
        public string? UserIdentifier { get; set; }

        /// <summary>
        /// The channel used in the execution.
        /// </summary>
        public string? Channel { get; set; }

        /// <summary>
        /// Validates the request parameters.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when no content is provided or any entry is null.</exception>
        public void Validate()
        {
            if (Content is null || Content.Count == 0 || Content.Contains(null!))
                throw new ArgumentException("At least one non-null text is required to generate embeddings.", nameof(Content));
        }
    }
}
