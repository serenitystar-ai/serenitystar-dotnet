using Microsoft.Extensions.DependencyInjection;
using SerenityStar.Models.Decisions;
using Xunit;

namespace SerenityStar.AIServices.IntegrationTests;

public class DecisionIntegrationTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly IDecisionClient _client;

    public DecisionIntegrationTests(TestFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.ServiceProvider.GetRequiredService<IDecisionClient>();
    }

    [Fact]
    public async Task EvaluateAsync_ShouldAnswerEveryQuestionInOrder()
    {
        // Arrange
        (string modelCode, string vendor) = _fixture.GetRequiredModel("Decision", "a decision model");
        DecisionReq request = new()
        {
            ModelCode = modelCode,
            Vendor = vendor,
            Input = "I was charged twice for my last invoice and nobody answers my emails. Please fix it today!",
            Questions =
            {
                new DecisionQuestion
                {
                    Type = DecisionQuestionType.Predicate,
                    Name = "is_complaint",
                    Instructions = "Is the customer complaining?"
                },
                new DecisionQuestion
                {
                    Type = DecisionQuestionType.Choice,
                    Name = "topic",
                    Instructions = "What is the message about?",
                    Choices = new List<DecisionChoice> { new() { Value = "billing" }, new() { Value = "shipping" }, new() { Value = "other" } }
                },
                new DecisionQuestion
                {
                    Type = DecisionQuestionType.Score,
                    Name = "urgency",
                    Instructions = "How urgent is the message?",
                    Levels = new List<DecisionScoreLevel> { new() { Label = "low" }, new() { Label = "medium" }, new() { Label = "high" } }
                }
            }
        };

        // Act
        DecisionResult result = await _client.EvaluateAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, result.InstanceId);
        Assert.Equal(new[] { "is_complaint", "topic", "urgency" }, result.Answers.Select(a => a.Name));
        Assert.DoesNotContain(result.Answers, a => a is UnsupportedDecisionAnswer);

        // The model may refuse a question, so each answer is checked only when it was answered.
        if (result.Answers[0] is PredicateDecisionAnswer predicate)
            Assert.InRange(predicate.Probability, 0, 1);

        if (result.Answers[1] is ChoiceDecisionAnswer choice)
            Assert.Contains(choice.Choice, new[] { "billing", "shipping", "other" });

        if (result.Answers[2] is ScoreDecisionAnswer score)
            Assert.InRange(score.Score, 0, 2);
    }

    [Fact]
    public async Task EvaluateAsync_WithImageAndAgentTraceInput_ShouldAnswer()
    {
        // Arrange
        (string modelCode, string vendor) = _fixture.GetRequiredModel("Decision", "a decision model");
        string imagePath = _fixture.GetRequiredFilePath("ImageFilePath", "an image (jpg, jpeg, png, gif or webp; max 20 MB)");
        DecisionReq request = new()
        {
            ModelCode = modelCode,
            Vendor = vendor,
            Input = DecisionInput.FromItems(
                DecisionInputItem.Message(DecisionMessageRole.User, DecisionContent.FromParts(
                    DecisionInputPart.InputText("Here is a picture. Can you look up my order 42?"),
                    DecisionInputPart.InputImage(await File.ReadAllBytesAsync(imagePath), GetImageMediaType(imagePath)))),
                DecisionInputItem.FunctionCall("call_1", "get_order", """{"orderId":42}"""),
                DecisionInputItem.FunctionCallOutput("call_1", """{"orderId":42,"status":"shipped"}"""),
                DecisionInputItem.Message(DecisionMessageRole.Assistant, "Your order 42 has been shipped.")),
            Questions =
            {
                new DecisionQuestion
                {
                    Type = DecisionQuestionType.Predicate,
                    Name = "used_order_tool",
                    Instructions = "Did the assistant look up the order with a tool before answering?"
                },
                new DecisionQuestion
                {
                    Type = DecisionQuestionType.Predicate,
                    Name = "has_image",
                    Instructions = "Did the user send an image?"
                }
            }
        };

        // Act
        DecisionResult result = await _client.EvaluateAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, result.InstanceId);
        Assert.Equal(new[] { "used_order_tool", "has_image" }, result.Answers.Select(a => a.Name));
        Assert.DoesNotContain(result.Answers, a => a is UnsupportedDecisionAnswer);

        foreach (PredicateDecisionAnswer predicate in result.Answers.OfType<PredicateDecisionAnswer>())
            Assert.InRange(predicate.Probability, 0, 1);
    }

    [Fact]
    public async Task EvaluateAsync_WithoutQuestions_ShouldThrowBeforeSending()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _client.EvaluateAsync(new DecisionReq { ModelCode = "decision-model", Vendor = "OpenAI", Input = "Hello" }));
    }

    private static string GetImageMediaType(string path) => Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".png" => "image/png",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".gif" => "image/gif",
        ".webp" => "image/webp",
        _ => throw new InvalidOperationException("'SerenityStar:ImageFilePath' must be a jpg, jpeg, png, gif or webp file.")
    };
}
