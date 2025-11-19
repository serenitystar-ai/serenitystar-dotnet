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

    public TestFixture()
    {
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var services = new ServiceCollection();

        var apiKey = Configuration["SerenityStar:ApiKey"];
        AssistantAgent = Configuration["SerenityStar:AssistantAgent"] ?? "assistantagent";
        ActivityAgent = Configuration["SerenityStar:ActivityAgent"] ?? "activityagent";

        // Check if we have a valid API key (not null, empty or the placeholder)
        HasValidApiKey = !string.IsNullOrEmpty(apiKey) && apiKey != "your-api-key-here";

        if (HasValidApiKey)
        {
            services.AddSerenityStar(apiKey);
        }
        else
        {
            // Add a placeholder service that will throw a meaningful exception when used
            services.AddSerenityStar("dummy-key-for-build");
            Console.WriteLine("WARNING: No valid API key found. Integration tests will be skipped.");
        }

        ServiceProvider = services.BuildServiceProvider();
    }

    public void Dispose()
    {
        if (ServiceProvider is IDisposable disposable)
            disposable.Dispose();
    }
}