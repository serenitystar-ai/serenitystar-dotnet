using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serenity.AIHub.SDK.NET.Extensions;
using System;
using System.IO;

namespace Serenity.AIHub.SDK.NET.IntegrationTests;

public class TestFixture : IDisposable
{
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

        var services = new ServiceCollection();

        var apiKey = Configuration["SerenityAIHub:ApiKey"];

        // Check if we have a valid API key (not null, empty or the placeholder)
        HasValidApiKey = !string.IsNullOrEmpty(apiKey) && apiKey != "your-api-key-here";

        if (HasValidApiKey)
        {
            services.AddSerenityAIHub(apiKey);
        }
        else
        {
            // Add a placeholder service that will throw a meaningful exception when used
            services.AddSerenityAIHub("dummy-key-for-build");
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