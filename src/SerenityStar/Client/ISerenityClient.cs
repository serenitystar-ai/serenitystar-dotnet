using SerenityStar.Agents;
using SerenityStar.Models.VolatileKnowledge;

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
        /// Uploads a file as volatile knowledge
        /// </summary>
        /// <param name="request">The upload request containing file stream and filename</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The created volatile knowledge entity</returns>
        Task<VolatileKnowledge> UploadVolatileKnowledgeAsync(
            UploadVolatileKnowledgeReq request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets the status of a volatile knowledge by ID
        /// </summary>
        /// <param name="knowledgeId">The ID of the volatile knowledge</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The volatile knowledge entity with current status</returns>
        Task<VolatileKnowledge> GetVolatileKnowledgeStatusAsync(
            string knowledgeId,
            CancellationToken cancellationToken = default);
    }
}