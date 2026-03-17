using Microsoft.Extensions.DependencyInjection;
using SerenityStar.Client;
using SerenityStar.Models.Transcription;
using Xunit;

namespace SerenityStar.IntegrationTests;

public class TranscriptionIntegrationTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly ISerenityClient _client;

    public TranscriptionIntegrationTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.ServiceProvider.GetRequiredService<ISerenityClient>();
    }

    private string GetAudioFilePath()
    {
        string? path = _fixture.AudioFilePath;

        if (string.IsNullOrEmpty(path))
            throw new InvalidOperationException(
                "No audio file path configured. Please set 'SerenityStar:AudioFilePath' in appsettings.Development.json " +
                "to the full path of an audio file on your machine (e.g., \"C:\\\\audio\\\\test.mp3\").");

        if (!File.Exists(path))
            throw new FileNotFoundException(
                $"Audio file not found at '{path}'. Please verify the path in 'SerenityStar:AudioFilePath'.");

        return path;
    }

    [Fact]
    public async Task TranscribeAsync_WithAudioFile_ShouldReturnTranscript()
    {
        // Arrange
        string audioPath = GetAudioFilePath();
        using FileStream fileStream = File.OpenRead(audioPath);
        TranscribeAudioReq request = new()
        {
            FileStream = fileStream,
            FileName = Path.GetFileName(audioPath)
        };

        // Act
        TranscribeResult result = await _client.AIServices.Transcription.TranscribeAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.InstanceId);
        Assert.NotNull(result.Transcript);
        Assert.NotEmpty(result.Transcript);
    }

    [Fact]
    public async Task TranscribeAsync_WithPrompt_ShouldReturnTranscript()
    {
        // Arrange
        string audioPath = GetAudioFilePath();
        using FileStream fileStream = File.OpenRead(audioPath);
        TranscribeAudioReq request = new()
        {
            FileStream = fileStream,
            FileName = Path.GetFileName(audioPath),
            Prompt = "Transcribe this audio recording in portuguese."
        };

        // Act
        TranscribeResult result = await _client.AIServices.Transcription.TranscribeAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Transcript);
        Assert.NotEmpty(result.Transcript);
    }

    [Fact]
    public async Task TranscribeAsync_WithUserIdentifier_ShouldReturnTranscript()
    {
        // Arrange
        string audioPath = GetAudioFilePath();
        using FileStream fileStream = File.OpenRead(audioPath);
        TranscribeAudioReq request = new()
        {
            FileStream = fileStream,
            FileName = Path.GetFileName(audioPath),
            UserIdentifier = "test-user-123",
            Channel = "SDK-Tests"
        };

        // Act
        TranscribeResult result = await _client.AIServices.Transcription.TranscribeAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Transcript);
        Assert.NotEmpty(result.Transcript);
    }

    [Fact]
    public async Task TranscribeAsync_WithModelId_ShouldReturnTranscript()
    {
        // Arrange
        Guid? modelId = _fixture.TranscriptionModelId;
        if (!modelId.HasValue)
            return; // Skip if no model ID configured

        string audioPath = GetAudioFilePath();
        using FileStream fileStream = File.OpenRead(audioPath);
        TranscribeAudioReq request = new()
        {
            ModelId = modelId,
            FileStream = fileStream,
            FileName = Path.GetFileName(audioPath)
        };

        // Act
        TranscribeResult result = await _client.AIServices.Transcription.TranscribeAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Transcript);
        Assert.NotEmpty(result.Transcript);
    }

    [Fact]
    public async Task TranscribeAsync_ShouldReturnTokenUsageAndCost()
    {
        // Arrange
        string audioPath = GetAudioFilePath();
        using FileStream fileStream = File.OpenRead(audioPath);
        TranscribeAudioReq request = new()
        {
            FileStream = fileStream,
            FileName = Path.GetFileName(audioPath)
        };

        // Act
        TranscribeResult result = await _client.AIServices.Transcription.TranscribeAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Transcript);

        if (result.TokenUsage != null)
            Assert.True(result.TokenUsage.TotalTokens >= 0);

        if (result.Cost != null)
        {
            Assert.True(result.Cost.Total >= 0);
            Assert.NotNull(result.Cost.Currency);
        }
    }

    [Fact]
    public async Task TranscribeAsync_ShouldReturnMetadata()
    {
        // Arrange
        string audioPath = GetAudioFilePath();
        using FileStream fileStream = File.OpenRead(audioPath);
        TranscribeAudioReq request = new()
        {
            FileStream = fileStream,
            FileName = Path.GetFileName(audioPath)
        };

        // Act
        TranscribeResult result = await _client.AIServices.Transcription.TranscribeAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Transcript);

        if (result.Metadata != null)
        {
            // Language should be a valid ISO 639-1 code if present
            if (result.Metadata.Language != null)
                Assert.True(result.Metadata.Language.Length >= 2);
        }
    }

    [Fact]
    public async Task TranscribeAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _client.AIServices.Transcription.TranscribeAsync(null!));
    }

    [Fact]
    public void TranscribeAudioReq_Validate_WithNullFileStream_ShouldThrowArgumentNullException()
    {
        // Arrange
        TranscribeAudioReq request = new()
        {
            FileName = "test.mp3"
        };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => request.Validate());
    }

    [Fact]
    public void TranscribeAudioReq_Validate_WithNullFileName_ShouldThrowArgumentNullException()
    {
        // Arrange
        using MemoryStream stream = new(new byte[] { 1, 2, 3 });
        TranscribeAudioReq request = new()
        {
            FileStream = stream
        };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => request.Validate());
    }

    [Fact]
    public async Task TranscribeByFileIdAsync_WithInvalidFileId_ShouldFail()
    {
        // Arrange
        TranscribeAudioByFileIdReq request = new()
        {
            FileId = Guid.NewGuid() // Non-existent file
        };

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            _client.AIServices.Transcription.TranscribeByFileIdAsync(request));
    }

    [Fact]
    public async Task TranscribeByFileIdAsync_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _client.AIServices.Transcription.TranscribeByFileIdAsync(null!));
    }
}
