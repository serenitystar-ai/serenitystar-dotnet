using Microsoft.Extensions.Options;
using SerenityStar.Agents;
using SerenityStar.Constants;
using SerenityStar.Models;
using SerenityStar.Models.Execute;
using SerenityStar.Models.VolatileKnowledge;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SerenityStar.Client
{
    /// <inheritdoc />
    public class SerenityClient : ISerenityClient
    {
        private readonly HttpClient _httpClient;
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Interact with the different agents available in Serenity Star.
        /// You can choose between assistants, activities, proxies, and chat completions.
        /// </summary>
        public AgentsScope Agents { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerenityClient"/> class for dependency injection.
        /// </summary>
        /// <param name="options">The options containing the API key.</param>
        /// <param name="httpClient">The HTTP client to use for making requests.</param>
        public SerenityClient(IOptions<SerenityStarOptions> options, HttpClient httpClient)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            string apiKey = options.Value.ApiKey ?? throw new ArgumentNullException(nameof(options), "The ApiKey property of the options. Value object is null.");
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            ConfigureHttpClient(_httpClient, apiKey);
            Agents = new AgentsScope(_httpClient);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerenityClient"/> class for direct instantiation.
        /// </summary>
        /// <param name="apiKey">The API key to use for authentication.</param>
        /// <param name="baseUrl">The base URL to use for the API.</param>
        /// <param name="timeout">The timeout in seconds.</param>
        public static SerenityClient Create(string apiKey, string? baseUrl = null, int? timeout = null)
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentNullException(nameof(apiKey));

            HttpClient httpClient = new HttpClient();
            ConfigureHttpClient(httpClient, apiKey, baseUrl, timeout);

            return new SerenityClient(httpClient);
        }

        // Private constructor for direct instantiation
        private SerenityClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            Agents = new AgentsScope(_httpClient);
        }

        private static void ConfigureHttpClient(HttpClient client, string apiKey, string? baseUrl = null, int? timeout = null)
        {
            client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
            baseUrl ??= ClientConstants.BaseUrl;
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(timeout ?? 100);
        }

        /// <inheritdoc />
        public async Task<AgentResult> Execute(string agentCode, List<ExecuteParameter>? input = null, int? agentVersion = null, int apiVersion = 2, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(agentCode))
                throw new ArgumentNullException(nameof(agentCode));

            input ??= new List<ExecuteParameter>();

            string version = agentVersion.HasValue ? $"/{agentVersion}" : string.Empty;

            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
                $"/api/v{apiVersion}/agent/{agentCode}/execute{version}",
                input,
                JsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
            }

            return await response.Content.ReadFromJsonAsync<AgentResult>(JsonOptions, cancellationToken)
                   ?? throw new InvalidOperationException("Failed to deserialize response");
        }

        /// <inheritdoc />
        public async Task<VolatileKnowledge> UploadVolatileKnowledgeAsync( // sarasa esto vuela de acá y revisar la lectura de la response (copiar del hub)
            UploadVolatileKnowledgeReq request,
            CancellationToken cancellationToken = default)
        {
            using MultipartFormDataContent content = new();
            StreamContent fileContent = new(request.FileStream);

            // Set the content type based on file extension
            string contentType = GetContentType(request.FileName);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

            content.Add(fileContent, "file", request.FileName);

            HttpResponseMessage response = await _httpClient.PostAsync(
                "volatileknowledge",
                content,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
            }

            string responseBody = await response.Content.ReadAsStringAsync();
            VolatileKnowledge result = JsonSerializer.Deserialize<VolatileKnowledge>(responseBody, JsonOptions);

            return result ?? throw new InvalidOperationException("Failed to deserialize volatile knowledge response");
        }

        private static string GetContentType(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLowerInvariant();
            return extension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".txt" => "text/plain",
                ".csv" => "text/csv",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".ppt" => "application/vnd.ms-powerpoint",
                ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                _ => "application/octet-stream"
            };
        }

        /// <inheritdoc />
        public async Task<VolatileKnowledge> GetVolatileKnowledgeStatusAsync( // sarasa esto vuela de acá y revisar la lectura de la response (copiar del hub)
            string knowledgeId,
            CancellationToken cancellationToken = default)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(
                $"volatileknowledge/{knowledgeId}",
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
            }

            string responseBody = await response.Content.ReadAsStringAsync();
            VolatileKnowledge result = JsonSerializer.Deserialize<VolatileKnowledge>(responseBody, JsonOptions);

            return result ?? throw new InvalidOperationException("Failed to deserialize volatile knowledge response");
        }
    }
}