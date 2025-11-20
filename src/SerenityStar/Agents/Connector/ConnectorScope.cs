using SerenityStar.Models.Connector;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SerenityStar.Agents.Connector
{
    /// <summary>
    /// Provides connector-related functionality for checking connection status.
    /// </summary>
    public static class ConnectorScope
    {
        /// <summary>
        /// Gets the connector status.
        /// </summary>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="agentCode">The agent code.</param>
        /// <param name="options">The connector status options.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The connector status.</returns>
        public static async Task<ConnectorStatusResult> GetConnectorStatusAsync(
            HttpClient httpClient,
            string agentCode,
            GetConnectorStatusReq options,
            CancellationToken cancellationToken = default)
        {
            JsonSerializerOptions jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            string url = $"/api/v2/agent/{agentCode}/connector/{options.ConnectorId}/status?agentInstanceId={options.AgentInstanceId}";

            HttpResponseMessage response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
            }

            return await response.Content.ReadFromJsonAsync<ConnectorStatusResult>(jsonOptions, cancellationToken)
                   ?? throw new InvalidOperationException("Failed to deserialize connector status");
        }
    }
}
