using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SerenityStar.Extensions;

namespace SerenityStar.AIServices.IntegrationTests;

public class TestFixture : IDisposable
{
    /// <summary>
    /// Placeholder used in appsettings.json for values each developer must set in appsettings.Development.json.
    /// </summary>
    public const string Placeholder = "replace-me";

    private const string DefaultBaseUrl = "https://api.serenitystar.ai";

    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly HttpClient _uploadHttpClient = new();

    public IServiceProvider ServiceProvider { get; }
    public IConfiguration Configuration { get; }
    public bool HasValidApiKey { get; }

    public TestFixture()
    {
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        ServiceCollection services = new();

        string? apiKey = GetSetting("ApiKey");


        // Check if we have a valid API key (not null, empty or a placeholder)
        HasValidApiKey = apiKey != null && apiKey != "your-api-key-here";

        if (!HasValidApiKey)
            throw new InvalidOperationException(
                "No valid API key found. Please set 'SerenityStar:ApiKey' in appsettings.Development.json or environment variables. " +
                "Integration tests require a valid Serenity Star API key to run.");

        _apiKey = apiKey!;

        // Production by default; an empty BaseUrl is treated as unset (AddSerenityStar only coalesces null,
        // and new Uri("") would throw).
        string? baseUrl = GetSetting("BaseUrl");
        _baseUrl = baseUrl ?? DefaultBaseUrl;

        services.AddSerenityStar(_apiKey, timeoutSeconds: 300, baseUrl: baseUrl)
                .AddSerenityStarAIServices();

        ServiceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// Reads 'SerenityStar:{key}', treating empty values and the placeholder as unset.
    /// </summary>
    public string? GetSetting(string key)
    {
        string? value = Configuration[$"SerenityStar:{key}"];
        return string.IsNullOrWhiteSpace(value) || value == Placeholder ? null : value;
    }

    /// <summary>
    /// Reads a setting the test cannot run without, failing with instructions when it is not configured.
    /// </summary>
    public string GetRequiredSetting(string key, string description) =>
        GetSetting(key) ?? throw new InvalidOperationException(
            $"'SerenityStar:{key}' is not configured. Please set it in appsettings.Development.json to {description}.");

    /// <summary>
    /// Reads a required model id.
    /// </summary>
    public Guid GetRequiredGuid(string key, string description)
    {
        string value = GetRequiredSetting(key, description);
        return Guid.TryParse(value, out Guid id)
            ? id
            : throw new InvalidOperationException($"'SerenityStar:{key}' must be a GUID ({description}).");
    }

    /// <summary>
    /// Reads an optional model id. Tests use the tenant's default model when it is not configured.
    /// </summary>
    public Guid? GetOptionalGuid(string key) =>
        Guid.TryParse(GetSetting(key), out Guid id) ? id : null;

    /// <summary>
    /// Reads a model selected by code and vendor from '{prefix}ModelCode' and '{prefix}Vendor'.
    /// </summary>
    public (string ModelCode, string Vendor) GetRequiredModel(string prefix, string description) =>
        GetOptionalModel(prefix) ?? throw new InvalidOperationException(
            $"'SerenityStar:{prefix}ModelCode' and 'SerenityStar:{prefix}Vendor' are not configured. " +
            $"Please set them in appsettings.Development.json to the code and vendor (e.g. \"OpenAI\") of {description}.");

    /// <summary>
    /// Reads an optional model selected by code and vendor. Tests use the tenant's default model when it is not configured.
    /// </summary>
    public (string ModelCode, string Vendor)? GetOptionalModel(string prefix)
    {
        string? modelCode = GetSetting($"{prefix}ModelCode");
        string? vendor = GetSetting($"{prefix}Vendor");

        if (modelCode is null && vendor is null)
            return null;

        // The API requires both values together.
        if (modelCode is null || vendor is null)
            throw new InvalidOperationException(
                $"'SerenityStar:{prefix}ModelCode' and 'SerenityStar:{prefix}Vendor' must be set together.");

        return (modelCode, vendor);
    }

    /// <summary>
    /// Reads the path of a local test file, failing with instructions when it is not configured or doesn't exist.
    /// </summary>
    public string GetRequiredFilePath(string key, string description)
    {
        string path = GetRequiredSetting(key, $"the full path of {description} on your machine");

        if (!File.Exists(path))
            throw new FileNotFoundException($"File not found at '{path}'. Please verify the path in 'SerenityStar:{key}'.");

        return path;
    }

    /// <summary>
    /// Uploads a local file to Serenity Star (as a private file) and returns its id, for the *ByFileId methods.
    /// </summary>
    /// <remarks>
    /// The SDK does not expose file upload publicly, so the tests call the API directly.
    /// </remarks>
    public async Task<Guid> UploadFileAsync(string path)
    {
        using MultipartFormDataContent content = new();
        StreamContent fileContent = new(File.OpenRead(path));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContent, "formFile", Path.GetFileName(path));

        using HttpRequestMessage request = new(HttpMethod.Post, new Uri(new Uri(_baseUrl), "/api/v2/file/upload")) { Content = content };
        request.Headers.Add("X-API-KEY", _apiKey);

        using HttpResponseMessage response = await _uploadHttpClient.SendAsync(request);
        string body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"File upload failed with status code {response.StatusCode}: {body}");

        using JsonDocument document = JsonDocument.Parse(body);
        return Guid.Parse(document.RootElement.GetProperty("id").GetString()!);
    }

    public void Dispose()
    {
        _uploadHttpClient.Dispose();

        if (ServiceProvider is IDisposable disposable)
            disposable.Dispose();
    }
}
