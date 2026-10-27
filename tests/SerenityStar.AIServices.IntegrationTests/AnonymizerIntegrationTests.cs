using Microsoft.Extensions.DependencyInjection;
using SerenityStar.Models.Anonymizer;
using Xunit;

namespace SerenityStar.AIServices.IntegrationTests;

public class AnonymizerIntegrationTests : IClassFixture<TestFixture>
{
    // Fictitious data only: never put real personal data in tests.
    private const string Email = "jane.doe@example.com";
    private const string Input = "Please contact Jane Doe at " + Email + ".";

    private readonly IAnonymizerClient _client;

    public AnonymizerIntegrationTests(TestFixture fixture)
    {
        _client = fixture.ServiceProvider.GetRequiredService<IAnonymizerClient>();
    }

    [Fact]
    public async Task ObfuscateAsync_ShouldReplaceSensitiveData()
    {
        // Act
        AnonymizerResult result = await _client.ObfuscateAsync(new AnonymizerReq { Inputs = Input });

        // Assert
        Assert.False(string.IsNullOrEmpty(result.Text));
        Assert.DoesNotContain(Email, result.Text);
        Assert.NotEmpty(result.Spans);
        Assert.All(result.Spans, span =>
        {
            Assert.False(string.IsNullOrEmpty(span.Placeholder));
            Assert.True(span.End > span.Start);
        });
    }

    [Fact]
    public async Task ObfuscateAsync_WithCategoryOptions_ShouldUseCustomKey()
    {
        // Arrange
        AnonymizerReq request = new()
        {
            Inputs = Input,
            Categories = new List<string> { "private_email" },
            CategoryOptions = new List<AnonymizerCategoryOption>
            {
                new() { Category = "private_email", Key = "Email" }
            }
        };

        // Act
        AnonymizerResult result = await _client.ObfuscateAsync(request);

        // Assert
        Assert.DoesNotContain(Email, result.Text);
        AnonymizerSpan span = Assert.Single(result.Spans);
        Assert.Equal("private_email", span.EntityGroup);
        // The API wraps the key, e.g. "<Email-1>".
        Assert.Contains("Email-", span.Placeholder);
    }

    [Fact]
    public async Task ObfuscateAsync_WithoutInputs_ShouldThrowBeforeSending()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _client.ObfuscateAsync(new AnonymizerReq()));
    }
}
