using Microsoft.Extensions.DependencyInjection;
using SerenityStar.Agents.Conversational;
using SerenityStar.Client;
using SerenityStar.Models.VolatileKnowledge;
using Xunit;

namespace SerenityStar.IntegrationTests;

/// <summary>
/// Integration tests for the agent-scoped volatile knowledge upload endpoints
/// (POST /api/agent/{agentCode}/volatileKnowledge and its variants).
/// </summary>
public class AgentVolatileKnowledgeTests : IClassFixture<TestFixture>
{
    private const string TestFileName = "test-document.txt";

    private readonly TestFixture _fixture;
    private readonly ISerenityClient _client;
    private readonly string _testFilePath;

    public AgentVolatileKnowledgeTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.ServiceProvider.GetRequiredService<ISerenityClient>();
        string baseDirectory = AppContext.BaseDirectory;
        string projectRoot = Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", ".."));
        _testFilePath = Path.Combine(projectRoot, TestFileName);
    }

    private void EnsureTestFileExists()
    {
        if (!File.Exists(_testFilePath))
            throw new FileNotFoundException($"Test file not found at {_testFilePath}");
    }

    [Fact]
    public async Task UploadForAgent_WithValidFile_ShouldAssociateAgentId()
    {
        // Arrange
        EnsureTestFileExists();
        Conversation conversation = _client.Agents.Assistants.CreateConversation(_fixture.AssistantAgent);

        using FileStream fileStream = File.OpenRead(_testFilePath);
        UploadVolatileKnowledgeReq request = new()
        {
            FileStream = fileStream,
            FileName = TestFileName
        };

        // Act
        VolatileKnowledgeRes result = await conversation.VolatileKnowledge.UploadForAgentAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.NotNull(result.Status);
    }

    [Fact]
    public async Task UploadForAgent_WithContent_ShouldSucceed()
    {
        // Arrange
        Conversation conversation = _client.Agents.Assistants.CreateConversation(_fixture.AssistantAgent);
        UploadVolatileKnowledgeReq request = new()
        {
            Content = "https://serenitystar.ai"
        };

        // Act
        VolatileKnowledgeRes result = await conversation.VolatileKnowledge.UploadForAgentAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task UploadFromBase64ForAgent_WithValidContent_ShouldSucceed()
    {
        // Arrange
        Conversation conversation = _client.Agents.Assistants.CreateConversation(_fixture.AssistantAgent);
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes("Sample volatile knowledge content for base64 upload.");
        UploadVolatileKnowledgeFromBase64Req request = new()
        {
            FileName = "base64-document.txt",
            MimeType = "text/plain",
            ContentBase64 = Convert.ToBase64String(bytes)
        };

        // Act
        VolatileKnowledgeRes result = await conversation.VolatileKnowledge.UploadFromBase64ForAgentAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task UploadFromFileForAgent_WithExistingFileId_ShouldSucceed()
    {
        // Arrange - first upload a file to obtain a file id
        EnsureTestFileExists();
        Conversation conversation = _client.Agents.Assistants.CreateConversation(_fixture.AssistantAgent);

        Guid fileId;
        using (FileStream fileStream = File.OpenRead(_testFilePath))
        {
            UploadVolatileKnowledgeReq seedRequest = new()
            {
                FileStream = fileStream,
                FileName = TestFileName
            };
            VolatileKnowledgeRes seed = await conversation.VolatileKnowledge.UploadForAgentAsync(seedRequest);
            Assert.True(seed.FileId.HasValue, "Expected a file id from the seed upload.");
            fileId = seed.FileId!.Value;
        }

        UploadVolatileKnowledgeFromFileReq request = new()
        {
            FileId = fileId
        };

        // Act
        VolatileKnowledgeRes result = await conversation.VolatileKnowledge.UploadFromFileForAgentAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public async Task GetAllowedMimeTypes_ShouldReturnNonEmptyList()
    {
        // Arrange
        Conversation conversation = _client.Agents.Assistants.CreateConversation(_fixture.AssistantAgent);

        // Act
        IReadOnlyList<string> mimeTypes = await conversation.VolatileKnowledge.GetAllowedMimeTypesAsync();

        // Assert
        Assert.NotNull(mimeTypes);
        Assert.NotEmpty(mimeTypes);
    }

    [Fact]
    public async Task UploadForAgent_WithUnsupportedFileType_ShouldThrowWithBackendError()
    {
        // Arrange
        Conversation conversation = _client.Agents.Assistants.CreateConversation(_fixture.AssistantAgent);
        byte[] bytes = { 0x00, 0x01, 0x02, 0x03 };
        using MemoryStream stream = new(bytes);
        UploadVolatileKnowledgeReq request = new()
        {
            FileStream = stream,
            FileName = "malicious.exe"
        };

        // Act & Assert - the backend rejects unsupported types with a 400 that the SDK surfaces.
        HttpRequestException ex = await Assert.ThrowsAsync<HttpRequestException>(
            () => conversation.VolatileKnowledge.UploadForAgentAsync(request));

        Assert.Contains("400", ex.Message);
    }

    [Fact]
    public async Task UploadForAgent_WithInvalidAgentCode_ShouldThrowNotFound()
    {
        // Arrange
        Conversation conversation = _client.Agents.Assistants.CreateConversation("non-existent-agent-code-xyz");
        UploadVolatileKnowledgeReq request = new()
        {
            Content = "https://serenitystar.ai"
        };

        // Act & Assert
        HttpRequestException ex = await Assert.ThrowsAsync<HttpRequestException>(
            () => conversation.VolatileKnowledge.UploadForAgentAsync(request));

        Assert.Contains("404", ex.Message);
    }
}
