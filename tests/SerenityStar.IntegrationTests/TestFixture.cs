using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SerenityStar.Extensions;

namespace SerenityStar.IntegrationTests;

public class TestFixture : IDisposable
{
    public IServiceProvider ServiceProvider { get; }
    public IConfiguration Configuration { get; }
    public bool HasValidApiKey { get; }
    public string AssistantAgent { get; }
    public string ActivityAgent { get; }
    public string ProxyAgent { get; }
    public string ChatCompletionAgent { get; }

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
        AssistantAgent = Configuration["SerenityStar:AssistantAgent"] ?? "assistantagent";
        ActivityAgent = Configuration["SerenityStar:ActivityAgent"] ?? "activityagent";
        ProxyAgent = Configuration["SerenityStar:ProxyAgent"] ?? "proxyagent";
        ChatCompletionAgent = Configuration["SerenityStar:ChatCompletionAgent"] ?? "chatcompletionagent";

        // Check if we have a valid API key (not null, empty or the placeholder)
        HasValidApiKey = !string.IsNullOrEmpty(apiKey) && apiKey != "your-api-key-here";

        if (!HasValidApiKey)
            throw new InvalidOperationException(
                "No valid API key found. Please set 'SerenityStar:ApiKey' in appsettings.Development.json or environment variables. " +
                "Integration tests require a valid Serenity Star API key to run.");

        services.AddSerenityStar(apiKey!);

        ServiceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        if (ServiceProvider is IDisposable disposable)
            disposable.Dispose();
    }
}