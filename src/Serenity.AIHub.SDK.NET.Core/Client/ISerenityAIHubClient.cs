using Serenity.AIHub.SDK.NET.Core.Models;
using Serenity.AIHub.SDK.NET.Core.Models.Execute;
using Serenity.AIHub.SDK.NET.Core.Models.VolatileKnowledge;

namespace Serenity.AIHub.SDK.NET.Core.Client;

/// <summary>
/// Represents the interface for interacting with the Serenity AI Hub.
/// </summary>
public interface ISerenityAIHubClient
{
    /// <summary>
    /// Creates a conversation with the specified agent code and version.
    /// </summary>
    /// <param name="agentCode">The agent code to use for the conversation.</param>
    /// <param name="inputParameters">Parameters to pass to the agent.</param>
    /// <param name="agentVersion">The version of the agent.</param>
    /// <param name="apiVersion">The API version to use (default is 2).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the created conversation response.</returns>
    Task<CreateConversationRes> CreateConversation(
    string agentCode,
    List<ExecuteParameter> inputParameters = null,
    int? agentVersion = null,
    int apiVersion = 2,
    CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes an agent.
    /// </summary>
    /// <param name="agentCode">The agent code.</param>
    /// <param name="input">Agent execution inputs</param>
    /// <param name="agentVersion">The version of the agent.</param>
    /// <param name="apiVersion">The API version to use (default is 2).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response from the agent.</returns>
    Task<AgentResult> Execute(string agentCode, List<ExecuteParameter> input = null, int? agentVersion = null, int apiVersion = 2, CancellationToken cancellationToken = default);

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
    /// Gets the status of a volatile knowledge by ID
    /// </summary>
    /// <param name="knowledgeId">The ID of the volatile knowledge</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The volatile knowledge entity with current status</returns>
    Task<VolatileKnowledge> GetVolatileKnowledgeStatusAsync(
        Guid knowledgeId,
        CancellationToken cancellationToken = default);
}