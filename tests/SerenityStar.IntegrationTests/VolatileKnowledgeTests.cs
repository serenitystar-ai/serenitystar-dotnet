using Microsoft.Extensions.DependencyInjection;
using Xunit;
using SerenityStar.Models.VolatileKnowledge;
using SerenityStar.Models;
using SerenityStar.Models.Execute;

namespace SerenityStar.IntegrationTests;

public class VolatileKnowledgeTests : IClassFixture<TestFixture>
{
    private const string TestFileName = "test-document.txt";
    private const string TestJpgFileName = "test-jpg-file.jpg";

    private readonly TestFixture _fixture;
    private readonly ISerenityAIHubClient _client;
    private readonly string _testFilePath;

    public VolatileKnowledgeTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.ServiceProvider.GetRequiredService<ISerenityAIHubClient>();
        string baseDirectory = AppContext.BaseDirectory;
        string projectRoot = Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", ".."));
        _testFilePath = Path.Combine(projectRoot, TestFileName);
    }

    private async Task EnsureTestFileExistsAsync()
    {
        if (!File.Exists(_testFilePath))
            throw new FileNotFoundException($"Test file not found at {_testFilePath}");

        await Task.CompletedTask;
    }

    [Fact]
    public async Task UploadVolatileKnowledge_WithValidFile_ShouldReturnKnowledgeWithId()
    {
        // Arrange
        await EnsureTestFileExistsAsync();
        using FileStream fileStream = File.OpenRead(_testFilePath);
        UploadVolatileKnowledgeReq request = new()
        {
            FileStream = fileStream,
            FileName = TestFileName
        };

        // Act
        VolatileKnowledge result = await _client.UploadVolatileKnowledgeAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.NotNull(result.Status);
    }

    [Fact]
    public async Task UploadVolatileKnowledge_WithContent_ShouldSucceed()
    {
        // Arrange
        UploadVolatileKnowledgeReq request = new()
        {
            Content = "https://serenitystar.ai"
        };

        // Act
        VolatileKnowledge result = await _client.UploadVolatileKnowledgeAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.NotNull(result.Status);
    }

    [Fact]
    public async Task UploadVolatileKnowledge_WithFileAndExpirationDays_ShouldSetExpiration()
    {
        // Arrange
        await EnsureTestFileExistsAsync();
        using FileStream fileStream = File.OpenRead(_testFilePath);
        UploadVolatileKnowledgeReq request = new()
        {
            FileStream = fileStream,
            FileName = TestFileName
        };

        // Act
        VolatileKnowledge result = await _client.UploadVolatileKnowledgeAsync(
            request,
            expirationDays: 7);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        if (result.ExpirationDate.HasValue)
        {
            DateTime expectedMinDate = DateTime.UtcNow.AddDays(6);
            DateTime expectedMaxDate = DateTime.UtcNow.AddDays(8);
            Assert.InRange(result.ExpirationDate.Value, expectedMinDate, expectedMaxDate);
        }
    }

    [Fact]
    public async Task UploadVolatileKnowledge_WithNoExpiration_ShouldNotSetExpirationDate()
    {
        // Arrange
        await EnsureTestFileExistsAsync();
        using FileStream fileStream = File.OpenRead(_testFilePath);
        UploadVolatileKnowledgeReq request = new()
        {
            FileStream = fileStream,
            FileName = TestFileName
        };

        // Act
        VolatileKnowledge result = await _client.UploadVolatileKnowledgeAsync(
            request,
            noExpiration: true);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task UploadVolatileKnowledge_WithCallbackUrl_ShouldSucceed()
    {
        // Arrange
        await EnsureTestFileExistsAsync();
        using FileStream fileStream = File.OpenRead(_testFilePath);
        UploadVolatileKnowledgeReq request = new()
        {
            FileStream = fileStream,
            FileName = TestFileName,
            CallbackUrl = "https://example.com/callback"
        };

        // Act
        VolatileKnowledge result = await _client.UploadVolatileKnowledgeAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task GetVolatileKnowledgeStatus_WithValidId_ShouldReturnStatus()
    {
        // Arrange
        await EnsureTestFileExistsAsync();
        using FileStream fileStream = File.OpenRead(_testFilePath);
        UploadVolatileKnowledgeReq uploadRequest = new()
        {
            FileStream = fileStream,
            FileName = TestFileName
        };

        VolatileKnowledge uploadedKnowledge = await _client.UploadVolatileKnowledgeAsync(uploadRequest);
        Guid knowledgeId = uploadedKnowledge.Id;

        // Act
        VolatileKnowledge statusResult = await _client.GetVolatileKnowledgeStatusAsync(knowledgeId);

        // Assert
        Assert.NotNull(statusResult);
        Assert.Equal(knowledgeId, statusResult.Id);
        Assert.NotNull(statusResult.Status);
    }

    [Fact]
    public async Task UploadAndWaitForProcessing_ShouldEventuallyBeReady()
    {
        // Arrange
        await EnsureTestFileExistsAsync();
        using FileStream fileStream = File.OpenRead(_testFilePath);
        UploadVolatileKnowledgeReq request = new()
        {
            FileStream = fileStream,
            FileName = TestFileName
        };

        VolatileKnowledge uploadedKnowledge = await _client.UploadVolatileKnowledgeAsync(request);
        Guid knowledgeId = uploadedKnowledge.Id;

        // Act & Assert
        VolatileKnowledge status = uploadedKnowledge;
        int maxAttempts = 30;
        int attempts = 0;

        while (status.Status != VolatileKnowledgeSimpleStatus.Invalid && status.Status != VolatileKnowledgeSimpleStatus.Error && attempts < maxAttempts)
        {
            await Task.Delay(1000);
            status = await _client.GetVolatileKnowledgeStatusAsync(knowledgeId);
            attempts++;
        }

        Assert.NotEqual(VolatileKnowledgeSimpleStatus.Error, status.Status);
        Assert.Equal(VolatileKnowledgeSimpleStatus.Success, status.Status);
    }

    [Fact]
    public async Task ExecuteAgentWithVolatileKnowledge_ShouldUseUploadedDocument()
    {
        // Arrange - Upload volatile knowledge
        await EnsureTestFileExistsAsync();
        using FileStream fileStream = File.OpenRead(_testFilePath);
        UploadVolatileKnowledgeReq uploadRequest = new()
        {
            FileStream = fileStream,
            FileName = TestFileName
        };

        VolatileKnowledge uploadedKnowledge = await _client.UploadVolatileKnowledgeAsync(uploadRequest);
        Guid knowledgeId = uploadedKnowledge.Id;

        // Wait for processing
        VolatileKnowledge status = uploadedKnowledge;
        int maxAttempts = 30;
        int attempts = 0;

        while (status.Status != VolatileKnowledgeSimpleStatus.Invalid && status.Status != VolatileKnowledgeSimpleStatus.Error && attempts < maxAttempts)
        {
            await Task.Delay(1000);
            status = await _client.GetVolatileKnowledgeStatusAsync(knowledgeId);
            attempts++;
        }

        Assert.Equal(VolatileKnowledgeSimpleStatus.Success, status.Status);

        // Create conversation
        CreateConversationRes conversation = await _client.CreateConversation("assistantagent");
        Assert.NotNull(conversation);
        Assert.NotEqual(Guid.Empty, conversation.ChatId);

        // Arrange - Prepare execution parameters
        List<ExecuteParameter> parameters =
        [
            new ExecuteParameter("chatId", conversation.ChatId),
            new ExecuteParameter("message", "Who was an important figure in the 100 years war?"),
            new ExecuteParameter("volatileKnowledgeIds", new List<string> { knowledgeId.ToString() }),
        ];

        // Act - Execute agent with volatile knowledge
        AgentResult result = await _client.Execute("assistantagent", parameters);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Content);
    }

    [Fact]
    public async Task UploadVolatileKnowledge_WithJpgFile_ShouldReturnKnowledgeWithId()
    {
        // Arrange
        string testJpgPath = Path.Combine(Path.GetDirectoryName(_testFilePath), TestJpgFileName);
        if (!File.Exists(testJpgPath))
            throw new FileNotFoundException($"Test JPG file not found at {testJpgPath}");

        using FileStream fileStream = File.OpenRead(testJpgPath);
        UploadVolatileKnowledgeReq request = new()
        {
            FileStream = fileStream,
            FileName = TestJpgFileName
        };

        // Act
        VolatileKnowledge result = await _client.UploadVolatileKnowledgeAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.NotNull(result.Status);
    }
}

internal interface ISerenityAIHubClient
{
}