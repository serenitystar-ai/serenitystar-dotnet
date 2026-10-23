using Microsoft.Extensions.DependencyInjection;
using SerenityStar.Models.AIServices;
using SerenityStar.Models.Ocr;
using Xunit;

namespace SerenityStar.AIServices.IntegrationTests;

public class OcrIntegrationTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly IOcrClient _client;

    // Optional: the tenant's default model is used when it is not configured.
    private readonly (string ModelCode, string Vendor)? _model;

    public OcrIntegrationTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.ServiceProvider.GetRequiredService<IOcrClient>();
        _model = _fixture.GetOptionalModel("Ocr");
    }

    private string GetOcrFilePath() => _fixture.GetRequiredFilePath("OcrFilePath", "a document with text (pdf, png, jpg or jpeg; max 50 MB)");

    private static void AssertSuccessfulExtraction(OcrExecutionResult result)
    {
        Assert.NotEqual(Guid.Empty, result.InstanceId);
        OcrFileResult file = Assert.Single(result.Files);
        Assert.True(file.Success, file.Error);
        Assert.NotNull(file.Result);
        Assert.NotEmpty(file.Result!.Pages!);
        Assert.False(string.IsNullOrWhiteSpace(file.Result.Markdown));
    }

    [Fact]
    public async Task ExtractAsync_WithFile_ShouldReturnMarkdown()
    {
        // Arrange
        string path = GetOcrFilePath();
        using FileStream fileStream = File.OpenRead(path);
        OcrReq request = new()
        {
            ModelCode = _model?.ModelCode,
            Vendor = _model?.Vendor,
            Files = { new AIServiceFile { FileStream = fileStream, FileName = Path.GetFileName(path) } }
        };

        // Act
        OcrExecutionResult result = await _client.ExtractAsync(request);

        // Assert
        AssertSuccessfulExtraction(result);
    }

    [Fact]
    public async Task ExtractFromBase64Async_ShouldReturnMarkdown()
    {
        // Arrange
        string path = GetOcrFilePath();
        OcrBase64Req request = new()
        {
            ModelCode = _model?.ModelCode,
            Vendor = _model?.Vendor,
            Base64Files =
            {
                new OcrBase64File
                {
                    ContentBase64 = Convert.ToBase64String(await File.ReadAllBytesAsync(path)),
                    MimeType = GetMimeType(path),
                    FileName = Path.GetFileName(path)
                }
            }
        };

        // Act
        OcrExecutionResult result = await _client.ExtractFromBase64Async(request);

        // Assert
        AssertSuccessfulExtraction(result);
    }

    [Fact]
    public async Task ExtractFromUrlsAsync_ShouldReturnMarkdown()
    {
        // Arrange
        OcrUrlReq request = new()
        {
            ModelCode = _model?.ModelCode,
            Vendor = _model?.Vendor,
            Urls = { _fixture.GetRequiredSetting("OcrUrl", "a public http(s) URL of a document with text (pdf, png, jpg or jpeg)") }
        };

        // Act
        OcrExecutionResult result = await _client.ExtractFromUrlsAsync(request);

        // Assert
        AssertSuccessfulExtraction(result);
    }

    [Fact]
    public async Task ExtractByFileIdAsync_WithUploadedFile_ShouldReturnMarkdown()
    {
        // Arrange
        Guid fileId = await _fixture.UploadFileAsync(GetOcrFilePath());
        OcrByFileIdReq request = new()
        {
            ModelCode = _model?.ModelCode,
            Vendor = _model?.Vendor,
            FileIds = { fileId }
        };

        // Act
        OcrExecutionResult result = await _client.ExtractByFileIdAsync(request);

        // Assert
        AssertSuccessfulExtraction(result);
    }

    [Fact]
    public async Task ExtractAsync_WithoutFiles_ShouldThrowBeforeSending()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _client.ExtractAsync(new OcrReq()));
    }

    private static string GetMimeType(string path) => Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".pdf" => "application/pdf",
        ".png" => "image/png",
        ".jpg" or ".jpeg" => "image/jpeg",
        _ => throw new InvalidOperationException("'SerenityStar:OcrFilePath' must be a pdf, png, jpg or jpeg file.")
    };
}
