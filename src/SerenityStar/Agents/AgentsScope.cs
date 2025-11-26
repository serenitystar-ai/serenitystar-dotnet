using SerenityStar.Agents.Conversational;
using SerenityStar.Agents.System;
using System.Net.Http;

namespace SerenityStar.Agents
{
    /// <summary>
    /// Provides access to all agent types.
    /// </summary>
    public sealed class AgentsScope
    {
        /// <summary>
        /// Interact with Activity agents.
        /// </summary>
        public ActivitiesScope Activities { get; }

        /// <summary>
        /// Interact with ChatCompletion agents.
        /// </summary>
        public ChatCompletionsScope ChatCompletions { get; }

        /// <summary>
        /// Interact with AI Proxy agents.
        /// </summary>
        public AIProxiesScope AIProxies { get; }

        /// <summary>
        /// Interact with Assistant agents.
        /// </summary>
        public AssistantsScope Assistants { get; }

        /// <summary>
        /// Interact with Copilot agents.
        /// </summary>
        public CopilotScope Copilots { get; }

        internal AgentsScope(HttpClient httpClient)
        {
            // Agents
            Activities = new ActivitiesScope(httpClient);
            ChatCompletions = new ChatCompletionsScope(httpClient);
            AIProxies = new AIProxiesScope(httpClient);
            Assistants = new AssistantsScope(httpClient);
            Copilots = new CopilotScope(httpClient);
        }
    }
}
