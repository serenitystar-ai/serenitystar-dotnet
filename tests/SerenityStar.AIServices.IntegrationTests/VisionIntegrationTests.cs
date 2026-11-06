using Microsoft.Extensions.DependencyInjection;
using SerenityStar.Models.Vision;
using Xunit;

namespace SerenityStar.AIServices.IntegrationTests;

public class VisionIntegrationTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly IVisionClient _client;

    // Optional: the tenant's default model is used when it is not configured.
    private readonly (string ModelCode, string Vendor)? _model;

    public VisionIntegrationTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.ServiceProvider.GetRequiredService<IVisionClient>();
        _model = _fixture.GetOptionalModel("Vision");
    }

    private string GetImageFilePath() => _fixture.GetRequiredFilePath("ImageFilePath", "an image (jpg, jpeg, png, gif or webp; max 20 MB)");

    [Fact]
    public async Task AnalyzeAsync_WithImageFile_ShouldReturnResponse()
    {
        // Arrange
        string imagePath = GetImageFilePath();
        using FileStream fileStream = File.OpenRead(imagePath);
        AnalyzeImageReq request = new()
        {
            ModelCode = _model?.ModelCode,
            Vendor = _model?.Vendor,
            FileStream = fileStream,
            FileName = Path.GetFileName(imagePath),
            Prompt = "Describe this image in one sentence."
        };

        // Act
        VisionResult result = await _client.AnalyzeAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, result.InstanceId);
        Assert.False(string.IsNullOrEmpty(result.Response));

        if (result.Cost != null)
            Assert.True(result.Cost.Total >= 0);
    }

    [Fact]
    public async Task AnalyzeByFileIdAsync_WithUploadedImage_ShouldReturnResponse()
    {
        // Arrange
        Guid fileId = await _fixture.UploadFileAsync(GetImageFilePath());
        AnalyzeImageByFileIdReq request = new()
        {
            ModelCode = _model?.ModelCode,
            Vendor = _model?.Vendor,
            FileId = fileId,
            Prompt = "Describe this image in one sentence."
        };

        // Act
        VisionResult result = await _client.AnalyzeByFileIdAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, result.InstanceId);
        Assert.False(string.IsNullOrEmpty(result.Response));
    }

    [Fact]
    public async Task AnalyzeAsync_WithoutFile_ShouldThrowBeforeSending()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _client.AnalyzeAsync(new AnalyzeImageReq { FileName = "image.png" }));
    }
}
