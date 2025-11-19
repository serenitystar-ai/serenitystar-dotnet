using Microsoft.Extensions.Options;
using Serenity.AIHub.SDK.NET.Core.Constants;
using Serenity.AIHub.SDK.NET.Core.Models;
using Serenity.AIHub.SDK.NET.Core.Models.Execute;
using Serenity.AIHub.SDK.NET.Core.Models.VolatileKnowledge;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Serenity.AIHub.SDK.NET.Core.Client;

/// <inheritdoc />
public class SerenityAIHubClient : ISerenityAIHubClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="SerenityAIHubClient"/> class for dependency injection.
    /// </summary>
    /// <param name="options">The options containing the API key.</param>
    /// <param name="httpClient">The HTTP client to use for making requests.</param>
    public SerenityAIHubClient(IOptions<SerenityAIHubOptions> options, HttpClient httpClient)
    {
        ArgumentNullException.ThrowIfNull(options);
        string apiKey = options.Value.ApiKey ?? throw new ArgumentNullException(nameof(options), "The ApiKey property of the options. Value object is null.");
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        ConfigureHttpClient(_httpClient, apiKey);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SerenityAIHubClient"/> class for direct instantiation.
    /// </summary>
    /// <param name="apiKey">The API key to use for authentication.</param>
    /// <param name="baseUrl">The base URL to use for the API.</param>
    /// <param name="timeout">The timeout in seconds.</param>
    public static SerenityAIHubClient Create(string apiKey, string baseUrl = null, int? timeout = null)
    {
        if (string.IsNullOrEmpty(apiKey))
            throw new ArgumentNullException(nameof(apiKey));

        HttpClient httpClient = new();
        ConfigureHttpClient(httpClient, apiKey, baseUrl, timeout);

        return new SerenityAIHubClient(httpClient);
    }

    // Private constructor for direct instantiation
    private SerenityAIHubClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private static void ConfigureHttpClient(HttpClient client, string apiKey, string baseUrl = null, int? timeout = null)
    {
        client.DefaultRequestHeaders.Add("X-API-KEY", apiKey);
        baseUrl ??= ClientConstants.BaseUrl;
        client.BaseAddress = new Uri(baseUrl);
        client.Timeout = TimeSpan.FromSeconds(timeout ?? 100);
    }

    /// <inheritdoc />
    public async Task<CreateConversationRes> CreateConversation(
    string agentCode,
    List<ExecuteParameter> inputParameters = null,
    int? agentVersion = null,
    int apiVersion = 2,
    CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(agentCode))
            throw new ArgumentNullException(nameof(agentCode));

        inputParameters ??= [];

        string version = agentVersion.HasValue ? $"/{agentVersion}" : string.Empty;

        // Create the content with input parameters
        var requestBody = new
        {
            inputParameters
        };

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            $"/api/v{apiVersion}/agent/{agentCode}/conversation{version}",
            requestBody,
            JsonOptions,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
        }

        return await response.Content.ReadFromJsonAsync<CreateConversationRes>(JsonOptions, cancellationToken)
               ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    /// <inheritdoc />
    public async Task<AgentResult> Execute(string agentCode, List<ExecuteParameter> input = null, int? agentVersion = null, int apiVersion = 2, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(agentCode))
            throw new ArgumentNullException(nameof(agentCode));

        input ??= [];

        string version = agentVersion.HasValue ? $"/{agentVersion}" : string.Empty;

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            $"/api/v{apiVersion}/agent/{agentCode}/execute{version}",
            input,
            JsonOptions,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
        }

        return await response.Content.ReadFromJsonAsync<AgentResult>(JsonOptions, cancellationToken)
               ?? throw new InvalidOperationException("Failed to deserialize response");
    }

    /// <summary>
    /// Uploads a file or content as volatile knowledge
    /// </summary>
    /// <param name="request">The upload request containing file stream and filename</param>
    /// <param name="processEmbeddings">Whether to process embeddings</param>
    /// <param name="noExpiration">Whether the knowledge should have no expiration</param>
    /// <param name="expirationDays">The number of days before expiration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created volatile knowledge entity</returns>
    public async Task<VolatileKnowledge> UploadVolatileKnowledgeAsync(
        UploadVolatileKnowledgeReq request,
        bool processEmbeddings = true,
        bool noExpiration = false,
        int? expirationDays = null,
        CancellationToken cancellationToken = default)
    {
        request.Validate();

        using MultipartFormDataContent content = new();

        // Add content if provided
        if (!string.IsNullOrEmpty(request.Content))
            content.Add(new StringContent(request.Content), "Content");

        // Add file if provided
        if (request.FileStream is not null)
        {
            StreamContent fileContent = new(request.FileStream);
            string contentType = GetContentType(request.FileName);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            content.Add(fileContent, "File", request.FileName);
        }

        // Add callback URL if provided
        if (!string.IsNullOrEmpty(request.CallbackUrl))
            content.Add(new StringContent(request.CallbackUrl), "CallbackUrl");

        // Build query parameters
        List<string> queryParams = new List<string>
        {
            $"processEmbeddings={processEmbeddings}",
            $"noExpiration={noExpiration}"
        };

        if (expirationDays.HasValue)
            queryParams.Add($"expirationDays={expirationDays.Value}");

        string queryString = string.Join("&", queryParams);
        string endpoint = $"volatileknowledge?{queryString}";

        HttpResponseMessage response = await _httpClient.PostAsync(
            endpoint,
            content,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
        }

        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
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
            ".md" => "text/markdown",
            ".jpg" => "image/jpg",
            ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            _ => throw new NotSupportedException($"File extension '{extension}' is not supported for upload."),
        };
    }

    /// <summary>
    /// Gets the status of a volatile knowledge by ID
    /// </summary>
    /// <param name="knowledgeId">The ID of the volatile knowledge</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The volatile knowledge entity with current status</returns>
    public async Task<VolatileKnowledge> GetVolatileKnowledgeStatusAsync(
        Guid knowledgeId,
        CancellationToken cancellationToken = default)
    {
        HttpResponseMessage response = await _httpClient.GetAsync(
            $"volatileknowledge/{knowledgeId}",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"Request failed with status code {response.StatusCode}: {errorContent}");
        }

        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        VolatileKnowledge result = JsonSerializer.Deserialize<VolatileKnowledge>(responseBody, JsonOptions);

        return result ?? throw new InvalidOperationException("Failed to deserialize volatile knowledge response");
    }
}