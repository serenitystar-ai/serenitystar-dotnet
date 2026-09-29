using SerenityStar.Client;
using SerenityStar.Models.ChatCompletion;
using SerenityStar.Models.Transcription;
using Xunit;

namespace SerenityStar.UnitTests;

/// <summary>
/// Invalid requests rejected by the SDK before any request is sent.
/// </summary>
public class RequestValidationTests
{
    [Fact]
    public void ChatCompletionReq_WithBothMessageAndAudioStream_Throws()
    {
        using MemoryStream audio = new(new byte[] { 1, 2, 3 });
        ChatCompletionReq request = new()
        {
            Message = "Hello",
            AudioFileStream = audio,
            AudioFileName = "test.wav"
        };

        Assert.Throws<ArgumentException>(() => request.Validate());
    }

    [Fact]
    public void ChatCompletionReq_WithNeitherMessageNorAudioStream_Throws()
    {
        ChatCompletionReq request = new();

        Assert.Throws<ArgumentException>(() => request.Validate());
    }

    [Fact]
    public void ChatCompletionReq_WithAudioStreamButNoFileName_Throws()
    {
        using MemoryStream audio = new(new byte[] { 1, 2, 3 });
        ChatCompletionReq request = new() { AudioFileStream = audio };

        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() => request.Validate());
        Assert.Equal(nameof(ChatCompletionReq.AudioFileName), ex.ParamName);
    }

    [Fact]
    public void TranscribeAudioReq_WithoutFileStream_Throws()
    {
        TranscribeAudioReq request = new() { FileName = "test.mp3" };

        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() => request.Validate());
        Assert.Equal(nameof(TranscribeAudioReq.FileStream), ex.ParamName);
    }

    [Fact]
    public void TranscribeAudioReq_WithoutFileName_Throws()
    {
        using MemoryStream audio = new(new byte[] { 1, 2, 3 });
        TranscribeAudioReq request = new() { FileStream = audio };

        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() => request.Validate());
        Assert.Equal(nameof(TranscribeAudioReq.FileName), ex.ParamName);
    }

    [Fact]
    public async Task TranscribeAsync_WithNullRequest_Throws()
    {
        SerenityClient client = NoRequestsClient();

        ArgumentNullException ex = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            client.AIServices.Transcription.TranscribeAsync(null!));
        Assert.Equal("request", ex.ParamName);
    }

    [Fact]
    public async Task GetMessageFeedbackAsync_WithEmptyAgentCode_Throws()
    {
        SerenityClient client = NoRequestsClient();

        ArgumentNullException ex = await Assert.ThrowsAsync<ArgumentNullException>(() =>
            client.Agents.Assistants.GetMessageFeedbackAsync(string.Empty));
        Assert.Equal("agentCode", ex.ParamName);
    }

    // Fails the test if the SDK sends a request instead of rejecting the call locally.
    private static SerenityClient NoRequestsClient()
        => TestClient.Create(_ => throw new InvalidOperationException("No request should be sent."));
}
