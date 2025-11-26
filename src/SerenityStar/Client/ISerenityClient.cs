using SerenityStar.Agents;

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
    }
}