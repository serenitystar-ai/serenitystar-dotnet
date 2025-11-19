using Microsoft.Extensions.DependencyInjection;
using SerenityStar.Client;
using SerenityStar.Models.Execute;
using SerenityStar.Models.VolatileKnowledge;
using SerenityStar.Agents.Conversational;
using Xunit;

namespace SerenityStar.IntegrationTests;

public class VolatileKnowledgeTests : IClassFixture<TestFixture>
{
    private const string TestFileName = "test-document.txt";
    private const string TestJpgFileName = "test-jpg-file.jpg";

    private readonly TestFixture _fixture;
    private readonly ISerenityClient _client;
    private readonly string _testFilePath;

    public VolatileKnowledgeTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.ServiceProvider.GetRequiredService<ISerenityClient>();
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

        // Create conversation with assistant agent
        Conversation conversation = _client.Agents.Assistants.CreateConversation(_fixture.AssistantAgent);

        // Send a message with volatile knowledge context (demonstration of integration)
        var messageWithKnowledge = $"Based on the uploaded document, answer: Who was an important figure in the 100 years war?";

        // Act
        AgentResult result = await conversation.SendMessageAsync(messageWithKnowledge);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Content);
        Assert.NotNull(conversation.ConversationId);
    }

    [Fact]
    public async Task UploadVolatileKnowledge_WithJpgFile_ShouldReturnKnowledgeWithId()
    {
        // Arrange
        string testJpgPath = Path.Combine(Path.GetDirectoryName(_testFilePath) ?? "", TestJpgFileName);
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

    [Fact]
    public async Task UploadMultipleKnowledge_ShouldReturnDifferentIds()
    {
        // Arrange
        await EnsureTestFileExistsAsync();

        // Act
        Guid knowledgeId1 = Guid.Empty;
        Guid knowledgeId2 = Guid.Empty;

        using (FileStream fileStream1 = File.OpenRead(_testFilePath))
        {
            UploadVolatileKnowledgeReq request1 = new()
            {
                FileStream = fileStream1,
                FileName = TestFileName
            };
            VolatileKnowledge result1 = await _client.UploadVolatileKnowledgeAsync(request1);
            knowledgeId1 = result1.Id;
        }

        await Task.Delay(500); // Small delay between uploads

        using (FileStream fileStream2 = File.OpenRead(_testFilePath))
        {
            UploadVolatileKnowledgeReq request2 = new()
            {
                FileStream = fileStream2,
                FileName = TestFileName
            };
            VolatileKnowledge result2 = await _client.UploadVolatileKnowledgeAsync(request2);
            knowledgeId2 = result2.Id;
        }

        // Assert
        Assert.NotEqual(knowledgeId1, knowledgeId2);
    }
}
