using Microsoft.Extensions.DependencyInjection;
using SerenityStar.Exceptions;
using SerenityStar.Models.SpeechGeneration;
using Xunit;

namespace SerenityStar.AIServices.IntegrationTests;

public class SpeechGenerationIntegrationTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly ISpeechGenerationClient _client;

    public SpeechGenerationIntegrationTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.ServiceProvider.GetRequiredService<ISpeechGenerationClient>();
    }

    private GenerateSpeechReq CreateRequest()
    {
        (string modelCode, string vendor) = _fixture.GetRequiredModel("SpeechGeneration", "a speech generation (text-to-speech) model");

        return new GenerateSpeechReq
        {
            ModelCode = modelCode,
            Vendor = vendor,
            Voice = _fixture.GetSetting("SpeechGenerationVoice"),
            Text = "Hello from the Serenity Star .NET SDK integration tests."
        };
    }

    [Fact]
    public async Task GenerateAsync_ShouldReturnAudioUrl()
    {
        // Act
        SpeechGenerationResult result = await _client.GenerateAsync(CreateRequest());

        // Assert
        Assert.NotEqual(Guid.Empty, result.InstanceId);
        Assert.False(string.IsNullOrEmpty(result.AudioUrl));

        if (result.Cost != null)
            Assert.True(result.Cost.Amount >= 0);
    }

    [Fact]
    public async Task StreamAsync_ShouldReturnStartAudioChunksAndStop()
    {
        // Arrange
        List<StreamingSpeechGenerationMessage> messages = new();

        // Act
        await foreach (StreamingSpeechGenerationMessage message in _client.StreamAsync(CreateRequest()))
            messages.Add(message);

        // Assert
        Assert.DoesNotContain(messages, m => m is StreamingSpeechGenerationMessageError);
        Assert.IsType<StreamingSpeechGenerationMessageStart>(messages.First());

        List<StreamingSpeechGenerationMessageAudio> chunks = messages.OfType<StreamingSpeechGenerationMessageAudio>().ToList();
        Assert.NotEmpty(chunks);
        Assert.True(chunks.Sum(c => c.GetAudioBytes().Length) > 0);

        StreamingSpeechGenerationMessageStop stop = Assert.IsType<StreamingSpeechGenerationMessageStop>(messages.Last());
        Assert.NotEqual(Guid.Empty, stop.InstanceId);
    }

    [Fact]
    public async Task GenerateAsync_WithNonExistentModel_ShouldThrowSerenityApiException()
    {
        // Arrange
        GenerateSpeechReq request = new() { ModelCode = "non-existent-model", Vendor = "OpenAI", Text = "Hello" };

        // Act & Assert
        await Assert.ThrowsAsync<SerenityApiException>(() => _client.GenerateAsync(request));
    }

    [Fact]
    public async Task GenerateAsync_WithoutText_ShouldThrowBeforeSending()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() => _client.GenerateAsync(new GenerateSpeechReq { ModelCode = "tts-1", Vendor = "OpenAI" }));
    }
}
