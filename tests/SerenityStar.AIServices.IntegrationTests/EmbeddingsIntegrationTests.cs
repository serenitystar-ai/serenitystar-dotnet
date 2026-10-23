using Microsoft.Extensions.DependencyInjection;
using SerenityStar.Models.Embeddings;
using Xunit;

namespace SerenityStar.AIServices.IntegrationTests;

public class EmbeddingsIntegrationTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly IEmbeddingClient _client;

    public EmbeddingsIntegrationTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.ServiceProvider.GetRequiredService<IEmbeddingClient>();
    }

    [Fact]
    public async Task GenerateAsync_ShouldReturnOneVectorPerText()
    {
        // Arrange
        GenerateEmbeddingsReq request = new()
        {
            ModelId = _fixture.GetOptionalGuid("EmbeddingModelId"),
            Content = { "The quick brown fox", "jumps over the lazy dog" }
        };

        // Act
        EmbeddingsResult result = await _client.GenerateAsync(request);

        // Assert
        Assert.Equal(2, result.Data.Count);
        Assert.Equal(new[] { 0, 1 }, result.Data.Select(d => d.Index).OrderBy(i => i));
        Assert.All(result.Data, d => Assert.NotEmpty(d.Embedding));

        // Every vector of the same model has the same dimensions.
        Assert.Single(result.Data.Select(d => d.Embedding.Length).Distinct());

        if (result.Cost != null)
            Assert.True(result.Cost.Total >= 0);
    }

    [Fact]
    public async Task GenerateAsync_WithoutContent_ShouldThrowBeforeSending()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _client.GenerateAsync(new GenerateEmbeddingsReq()));
    }
}
