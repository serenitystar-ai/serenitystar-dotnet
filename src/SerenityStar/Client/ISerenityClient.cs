using SerenityStar.Agents;
using SerenityStar.Models.VolatileKnowledge;
using SerenityStar.Models.Execute;
using System;
using System.Collections.Generic;
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
        /// Executes an agent synchronously.
        /// </summary>
        /// <param name="agentCode">The code of the agent to execute.</param>
        /// <param name="input">Optional input parameters for the agent.</param>
        /// <param name="agentVersion">Optional specific version of the agent to execute.</param>
        /// <param name="apiVersion">The API version to use (default is 2).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of the agent execution.</returns>
        Task<AgentResult> Execute(
            string agentCode,
            List<ExecuteParameter>? input = null,
            int? agentVersion = null,
            int apiVersion = 2,
            CancellationToken cancellationToken = default);

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
        /// Gets the status of a volatile knowledge entity by its ID.
        /// </summary>
        /// <param name="knowledgeId">The knowledge ID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The volatile knowledge entity with current status.</returns>
        Task<VolatileKnowledge> GetVolatileKnowledgeStatusAsync(
            Guid knowledgeId,
            CancellationToken cancellationToken = default);
    }
}