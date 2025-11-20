using SerenityStar.Agents.Conversational;
using SerenityStar.Agents.System;
using SerenityStar.Agents.VolatileKnowledge;
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
        /// Interact with Proxy agents.
        /// </summary>
        public ProxiesScope Proxies { get; }

        /// <summary>
        /// Interact with Assistant agents.
        /// </summary>
        public AssistantsScope Assistants { get; }

        /// <summary>
        /// Manage volatile knowledge.
        /// </summary>
        public VolatileKnowledgeScope VolatileKnowledge { get; }

        internal AgentsScope(HttpClient httpClient)
        {
            // Agents
            Activities = new ActivitiesScope(httpClient);
            ChatCompletions = new ChatCompletionsScope(httpClient);
            Proxies = new ProxiesScope(httpClient);
            Assistants = new AssistantsScope(httpClient);

            VolatileKnowledge = new VolatileKnowledgeScope(httpClient);
        }
    }
}
