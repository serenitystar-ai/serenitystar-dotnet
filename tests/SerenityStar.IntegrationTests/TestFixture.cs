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
    public string CopilotAgent { get; }
    public string ActivityAgent { get; }
    public string ProxyAgent { get; }
    public string ChatCompletionAgent { get; }
    public string ApprovalProxyAgent { get; }
    public string ApprovalMessage { get; }
    public string UserChoiceAssistantAgent { get; }
    public string UserChoiceProxyAgent { get; }
    public string UserChoiceMessage { get; }
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
        AssistantAgent = Configuration["SerenityStar:AssistantAgent"] ?? "assistantagent";
        CopilotAgent = Configuration["SerenityStar:CopilotAgent"] ?? "copilotagent";
        ActivityAgent = Configuration["SerenityStar:ActivityAgent"] ?? "activityagent";
        ProxyAgent = Configuration["SerenityStar:ProxyAgent"] ?? "proxyagent";
        ChatCompletionAgent = Configuration["SerenityStar:ChatCompletionAgent"] ?? "chatcompletionagent";

        // The skill approval tests use AssistantAgent and this proxy, both with at least one skill that requires
        // approval, and a message that makes them invoke it.
        ApprovalProxyAgent = Configuration["SerenityStar:ApprovalProxyAgent"] ?? "aiproxyagenttester";
        ApprovalMessage = Configuration["SerenityStar:ApprovalMessage"] ?? "Generate an image of a red apple on a white table.";

        // The Request User Choice tests use an assistant agent with the skill enabled (the proxy enables it per
        // request) and a message that makes them ask the user to choose.
        UserChoiceAssistantAgent = Configuration["SerenityStar:UserChoiceAssistantAgent"] ?? AssistantAgent;
        UserChoiceProxyAgent = Configuration["SerenityStar:UserChoiceProxyAgent"] ?? ApprovalProxyAgent;
        UserChoiceMessage = Configuration["SerenityStar:UserChoiceMessage"]
            ?? "I want to plan a trip. Before answering, use the request user choice skill to ask me which destination type I prefer (beach, mountain, city) and my budget range, giving me options.";

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

        services.AddSerenityStar(apiKey!, baseUrl: baseUrl);

        ServiceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        if (ServiceProvider is IDisposable disposable)
            disposable.Dispose();
    }
}