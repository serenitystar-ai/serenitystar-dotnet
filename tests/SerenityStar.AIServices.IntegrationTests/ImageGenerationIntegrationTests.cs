using Microsoft.Extensions.DependencyInjection;
using SerenityStar.Models.AIServices;
using SerenityStar.Models.ImageGeneration;
using Xunit;

namespace SerenityStar.AIServices.IntegrationTests;

public class ImageGenerationIntegrationTests : IClassFixture<TestFixture>
{
    private const string SourceImagesModelDescription = "the id of an image generation model that accepts source images";

    private readonly TestFixture _fixture;
    private readonly IImageGenerationClient _client;

    public ImageGenerationIntegrationTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.ServiceProvider.GetRequiredService<IImageGenerationClient>();
    }

    private string GetImageFilePath() => _fixture.GetRequiredFilePath("ImageFilePath", "an image (jpg, jpeg, png, gif or webp; max 20 MB)");

    [Fact]
    public async Task GenerateAsync_ShouldReturnImageUrl()
    {
        // Arrange
        GenerateImageReq request = new()
        {
            ModelId = _fixture.GetOptionalGuid("ImageGenerationModelId"),
            Prompt = "A simple flat icon of a blue circle on a white background"
        };

        // Act
        ImageGenerationResult result = await _client.GenerateAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, result.InstanceId);
        Assert.False(string.IsNullOrEmpty(result.ImageUrl));
    }

    [Fact]
    public async Task GenerateWithSourceImagesAsync_ShouldReturnImageUrl()
    {
        // Arrange
        string imagePath = GetImageFilePath();
        using FileStream fileStream = File.OpenRead(imagePath);
        GenerateImageWithSourceImagesReq request = new()
        {
            ModelId = _fixture.GetRequiredGuid("ImageGenerationWithSourceImagesModelId", SourceImagesModelDescription),
            Prompt = "Turn this image into black and white",
            Files = { new AIServiceFile { FileStream = fileStream, FileName = Path.GetFileName(imagePath) } }
        };

        // Act
        ImageGenerationResult result = await _client.GenerateWithSourceImagesAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, result.InstanceId);
        Assert.False(string.IsNullOrEmpty(result.ImageUrl));
    }

    [Fact]
    public async Task GenerateByFileIdAsync_WithUploadedImage_ShouldReturnImageUrl()
    {
        // Arrange
        Guid fileId = await _fixture.UploadFileAsync(GetImageFilePath());
        GenerateImageByFileIdReq request = new()
        {
            ModelId = _fixture.GetRequiredGuid("ImageGenerationWithSourceImagesModelId", SourceImagesModelDescription),
            Prompt = "Turn this image into black and white",
            FileIds = { fileId }
        };

        // Act
        ImageGenerationResult result = await _client.GenerateByFileIdAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, result.InstanceId);
        Assert.False(string.IsNullOrEmpty(result.ImageUrl));
    }

    [Fact]
    public async Task GenerateAsync_WithoutPrompt_ShouldThrowBeforeSending()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _client.GenerateAsync(new GenerateImageReq()));
    }
}
