using Microsoft.Extensions.DependencyInjection;
using SerenityStar.Exceptions;
using SerenityStar.Files;
using SerenityStar.Models.Files;
using Xunit;

namespace SerenityStar.AIServices.IntegrationTests;

public class FileIntegrationTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly IFileClient _client;

    public FileIntegrationTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.ServiceProvider.GetRequiredService<IFileClient>();
    }

    [Fact]
    public async Task UploadAsync_WithFile_ShouldReturnFileId()
    {
        // Arrange
        string path = _fixture.GetRequiredFilePath("OcrFilePath", "a document with text (pdf, png, jpg or jpeg; max 50 MB)");
        using FileStream fileStream = File.OpenRead(path);
        UploadFileReq request = new()
        {
            FileStream = fileStream,
            FileName = Path.GetFileName(path)
        };

        // Act
        UploadFileResult result = await _client.UploadAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(Path.GetFileName(path), result.Name);
        Assert.False(string.IsNullOrEmpty(result.Extension));
    }

    [Fact]
    public async Task UploadFromUrlAsync_WithPublicUrl_ShouldReturnFileId()
    {
        // Arrange
        UploadFileFromUrlReq request = new()
        {
            FileUrl = _fixture.GetRequiredSetting("OcrUrl", "a public http(s) URL of a document with text"),
            FileName = "document-from-url.pdf"
        };

        // Act
        UploadFileResult result = await _client.UploadFromUrlAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("document-from-url.pdf", result.Name);
    }

    [Fact]
    public async Task UploadFromUrlAsync_WithUnreachableUrl_ShouldThrowSerenityApiException()
    {
        // Arrange
        UploadFileFromUrlReq request = new()
        {
            FileUrl = "https://invalid.serenitystar.example/missing.pdf"
        };

        // Act & Assert
        await Assert.ThrowsAsync<SerenityApiException>(() => _client.UploadFromUrlAsync(request));
    }
}
