using SerenityStar.Agents;
using SerenityStar.Models.VolatileKnowledge;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SerenityStar.Client
{
    /// <summary>
    /// Represents the interface for interacting with Serenity Star.
    /// </summary>
    public interface ISerenityClient
    {
        /// <summary>
        /// Gets the agents scope for accessing all agent types.
        /// </summary>
        AgentsScope Agents { get; }

        /// <summary>
        /// Uploads a file or content as volatile knowledge
        /// </summary>
        /// <param name="request">The upload request containing file stream or content</param>
        /// <param name="processEmbeddings">Optional parameter to indicate whether to process embeddings. Default is true.</param>
        /// <param name="noExpiration">Optional parameter to indicate whether the knowledge should not expire. Default is false.</param>
        /// <param name="expirationDays">Optional parameter to specify the number of days until expiration.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The created volatile knowledge entity</returns>
        Task<VolatileKnowledge> UploadVolatileKnowledgeAsync(
            UploadVolatileKnowledgeReq request,
            bool processEmbeddings = true,
            bool noExpiration = false,
            int? expirationDays = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The volatile knowledge entity with current status</returns>
        Task<VolatileKnowledge> GetVolatileKnowledgeStatusAsync(
            Guid knowledgeId,
            CancellationToken cancellationToken = default);
    }
}