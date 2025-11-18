using SerenityStar.Agents.Assistants;
using SerenityStar.Agents.System;
using System.Net.Http;

namespace SerenityStar.Agents
{
    /// <summary>
    /// Provides access to all agent types.
    /// </summary>
    public class AgentsScope
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

        internal AgentsScope(HttpClient httpClient)
        {
            Activities = new ActivitiesScope(httpClient);
            ChatCompletions = new ChatCompletionsScope(httpClient);
            Proxies = new ProxiesScope(httpClient);
            Assistants = new AssistantsScope(httpClient);
        }
    }
}
