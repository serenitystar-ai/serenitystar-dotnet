using SerenityStar.Models.AIServices;
using System;
using System.Collections.Generic;

namespace SerenityStar.Models.Embeddings
{
    /// <summary>
    /// Represents the result of an embeddings generation operation.
    /// </summary>
    public sealed class EmbeddingsResult
    {
        /// <summary>
        /// One embedding per input text.
        /// </summary>
        public List<EmbeddingData> Data { get; set; } = new List<EmbeddingData>();

        /// <summary>
        /// The cost information for this execution in the tenant's currency.
        /// </summary>
        public EmbeddingsCost? Cost { get; set; }

        /// <summary>
        /// The billed quantities in the units the model is priced by.
        /// </summary>
        public AIServiceUsage? Usage { get; set; }
    }

    /// <summary>
    /// The embedding of a single input text.
    /// </summary>
    public sealed class EmbeddingData
    {
        /// <summary>
        /// The position of the input text in the request.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The embedding vector.
        /// </summary>
        public float[] Embedding { get; set; } = Array.Empty<float>();
    }

    /// <summary>
    /// The cost of an embeddings generation in the tenant's currency.
    /// </summary>
    public sealed class EmbeddingsCost
    {
        /// <summary>
        /// The total cost in the tenant's currency.
        /// </summary>
        public double Total { get; set; }

        /// <summary>
        /// The ISO currency code for the tenant's currency.
        /// </summary>
        public string? Currency { get; set; }
    }
}
