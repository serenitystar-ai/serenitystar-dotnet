using SerenityStar.Agents;
using SerenityStar.AIServices;

namespace SerenityStar.Client
{
    /// <summary>
    /// Represents the interface for interacting with Serenity Star.
    /// This interface is intended for dependency injection only and should not be implemented by consumers.
    /// </summary>
    public interface ISerenityClient
    {
        /// <summary>
        /// Gets the agents scope for accessing all agent types.
        /// </summary>
        AgentsScope Agents { get; }

        /// <summary>
        /// Gets the AI services scope for accessing transcription, speech, and other AI services.
        /// </summary>
        AIServicesScope AIServices { get; }
    }
}