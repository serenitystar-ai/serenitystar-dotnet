using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SerenityStar.Extensions;

namespace SerenityStar.AIServices.IntegrationTests;

public class TestFixture : IDisposable
{
    public IServiceProvider ServiceProvider { get; }
    public IConfiguration Configuration { get; }
    public bool HasValidApiKey { get; }
    public Guid? TranscriptionModelId { get; }
    public string? AudioFilePath { get; }

    public TestFixture()
    {
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        ServiceCollection services = new();

        string? apiKey = Configuration["SerenityStar:ApiKey"];

        string? transcriptionModelId = Configuration["SerenityStar:TranscriptionModelId"];
        if (!string.IsNullOrEmpty(transcriptionModelId) && Guid.TryParse(transcriptionModelId, out Guid modelId))
            TranscriptionModelId = modelId;

        AudioFilePath = Configuration["SerenityStar:AudioFilePath"];

        // Check if we have a valid API key (not null, empty or the placeholder)
        HasValidApiKey = !string.IsNullOrEmpty(apiKey) && apiKey != "your-api-key-here";

        if (!HasValidApiKey)
            throw new InvalidOperationException(
                "No valid API key found. Please set 'SerenityStar:ApiKey' in appsettings.Development.json or environment variables. " +
                "Integration tests require a valid Serenity Star API key to run.");

        // Treat an empty BaseUrl as unset so the SDK falls back to its default endpoint
        // (AddSerenityStar only coalesces null, and new Uri("") would throw).
        string? baseUrl = Configuration["SerenityStar:BaseUrl"];
        if (string.IsNullOrEmpty(baseUrl))
            baseUrl = null;

        services.AddSerenityStar(apiKey!, baseUrl: baseUrl)
                .AddSerenityStarAIServices();

        ServiceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        if (ServiceProvider is IDisposable disposable)
            disposable.Dispose();
    }
}
