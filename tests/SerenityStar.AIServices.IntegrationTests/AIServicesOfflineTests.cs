using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using SerenityStar.Client;
using SerenityStar.Exceptions;
using SerenityStar.Extensions;
using SerenityStar.Models.AIServices;
using SerenityStar.Models.Anonymizer;
using SerenityStar.Models.Decisions;
using SerenityStar.Models.Embeddings;
using SerenityStar.Models.ImageGeneration;
using SerenityStar.Models.Ocr;
using Xunit;

namespace SerenityStar.AIServices.IntegrationTests
{
    /// <summary>
    /// Offline tests for the AI service clients. These use a fake <see cref="HttpMessageHandler"/> to capture the
    /// outgoing request and return a canned response, so they do not require an API key or network access.
    /// </summary>
    public class AIServicesOfflineTests
    {
        private const string BaseUrl = "https://offline.example.com";

        /// <summary>
        /// Captures each outgoing request (including multipart fields) and returns the configured response.
        /// </summary>
        private sealed class CapturingHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode _statusCode;
            private readonly string _responseBody;
            private readonly string _responseMediaType;

            public CapturingHandler(string responseBody, string responseMediaType = "application/json", HttpStatusCode statusCode = HttpStatusCode.OK)
            {
                _responseBody = responseBody;
                _responseMediaType = responseMediaType;
                _statusCode = statusCode;
            }

            public int RequestCount { get; private set; }
            public HttpMethod? Method { get; private set; }
            public Uri? RequestUri { get; private set; }
            public string[] ApiKeyValues { get; private set; } = Array.Empty<string>();
            public string? ContentMediaType { get; private set; }
            public JsonElement? JsonBody { get; private set; }
            public List<(string Name, string? FileName, string? MediaType, string? Value)> FormParts { get; } = new();

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                RequestCount++;
                Method = request.Method;
                RequestUri = request.RequestUri;
                ApiKeyValues = request.Headers.TryGetValues("X-API-KEY", out IEnumerable<string>? values) ? values.ToArray() : Array.Empty<string>();
                ContentMediaType = request.Content?.Headers.ContentType?.MediaType;

                if (request.Content is MultipartFormDataContent multipart)
                {
                    foreach (HttpContent part in multipart)
                    {
                        string name = part.Headers.ContentDisposition!.Name!.Trim('"');
                        string? fileName = part.Headers.ContentDisposition.FileName?.Trim('"');
                        string? value = fileName is null ? await part.ReadAsStringAsync(cancellationToken) : null;
                        FormParts.Add((name, fileName, part.Headers.ContentType?.MediaType, value));
                    }
                }
                else if (request.Content != null)
                {
                    using JsonDocument document = JsonDocument.Parse(await request.Content.ReadAsStringAsync(cancellationToken));
                    JsonBody = document.RootElement.Clone();
                }

                return new HttpResponseMessage(_statusCode)
                {
                    Content = new StringContent(_responseBody, Encoding.UTF8, _responseMediaType)
                };
            }

            public string? FormValue(string name) => FormParts.SingleOrDefault(p => p.Name == name).Value;
        }

        private static SerenityClient CreateClient(CapturingHandler handler) =>
            new SerenityClientBuilder()
                .WithApiKey("test-key")
                .WithBaseUrl(BaseUrl)
                .WithHttpClient(new HttpClient(handler))
                .Build();

        private static Stream FileStreamOf(string content) => new MemoryStream(Encoding.UTF8.GetBytes(content));

        private static void AssertPost(CapturingHandler handler, string path)
        {
            Assert.Equal(1, handler.RequestCount);
            Assert.Equal(HttpMethod.Post, handler.Method);
            Assert.Equal($"{BaseUrl}{path}", handler.RequestUri!.AbsoluteUri);
            Assert.Equal(new[] { "test-key" }, handler.ApiKeyValues);
        }

        #region Anonymizer
        [Fact]
        public async Task Anonymizer_Obfuscate_SendsJsonAndParsesSnakeCaseSpans()
        {
            CapturingHandler handler = new("""{"text":"Hi Person-1","spans":[{"entity_group":"private_person","key":"Person","word":"John","start":3,"end":7,"score":0.98,"placeholder":"Person-1"}],"provider":"OpenAIPrivacyFilter"}""");

            AnonymizerResult result = await CreateClient(handler).GetAnonymizerClient().ObfuscateAsync(new AnonymizerReq
            {
                Inputs = "Hi John",
                Categories = new List<string> { "private_person" },
                CategoryOptions = new List<AnonymizerCategoryOption> { new() { Category = "private_person", Key = "Person", Threshold = 0.5 } }
            });

            AssertPost(handler, "/api/v2/anonymizer/obfuscate");
            JsonElement body = handler.JsonBody!.Value;
            Assert.Equal("Hi John", body.GetProperty("inputs").GetString());
            Assert.Equal("private_person", body.GetProperty("categories")[0].GetString());
            Assert.Equal(0.5, body.GetProperty("categoryOptions")[0].GetProperty("threshold").GetDouble());
            Assert.False(body.TryGetProperty("vendor", out _));

            Assert.Equal("Hi Person-1", result.Text);
            AnonymizerSpan span = Assert.Single(result.Spans);
            Assert.Equal("private_person", span.EntityGroup);
            Assert.Equal("Person-1", span.Placeholder);
            Assert.Equal(7, span.End);
        }
        #endregion

        #region Decisions
        [Fact]
        public async Task Decisions_Evaluate_SendsQuestionsAndParsesPolymorphicAnswers()
        {
            CapturingHandler handler = new("""
                {"instanceId":"11111111-1111-1111-1111-111111111111","answers":[
                  {"type":"predicate","name":"is_complaint","probability":0.9},
                  {"type":"choice","name":"topic","choice":"billing","probabilities":[{"value":"billing","probability":0.8},{"value":"other","probability":0.2}],"confidence":0.7},
                  {"type":"score","name":"urgency","score":1.4,"probabilities":[{"value":0,"label":"low","probability":0.1}],"confidence":0.6},
                  {"type":"refusal","name":"skipped"},
                  {"type":"ranking","name":"future"}],
                 "cost":{"amount":0.02,"currency":"USD"}}
                """);

            DecisionResult result = await CreateClient(handler).GetDecisionClient().EvaluateAsync(new DecisionReq
            {
                AIModelId = Guid.NewGuid(),
                Input = "My invoice is wrong!",
                Questions =
                {
                    new DecisionQuestion { Type = DecisionQuestionType.Predicate, Name = "is_complaint", Instructions = "Is it a complaint?" },
                    new DecisionQuestion
                    {
                        Type = DecisionQuestionType.Choice, Name = "topic", Instructions = "Topic",
                        Choices = new List<DecisionChoice> { new() { Value = "billing" }, new() { Value = "other" } }
                    }
                }
            });

            AssertPost(handler, "/api/v2/decisions");
            Assert.Equal("My invoice is wrong!", handler.JsonBody!.Value.GetProperty("input").GetString());
            JsonElement questions = handler.JsonBody!.Value.GetProperty("questions");
            Assert.Equal("predicate", questions[0].GetProperty("type").GetString());
            Assert.False(questions[0].TryGetProperty("choices", out _));
            Assert.Equal("billing", questions[1].GetProperty("choices")[0].GetProperty("value").GetString());

            Assert.Equal(0.9, Assert.IsType<PredicateDecisionAnswer>(result.Answers[0]).Probability);
            ChoiceDecisionAnswer choice = Assert.IsType<ChoiceDecisionAnswer>(result.Answers[1]);
            Assert.Equal("billing", choice.Choice);
            Assert.Equal(2, choice.Probabilities.Count);
            Assert.Equal("low", Assert.IsType<ScoreDecisionAnswer>(result.Answers[2]).Probabilities[0].Label);
            Assert.Equal("skipped", Assert.IsType<RefusalDecisionAnswer>(result.Answers[3]).Name);
            UnsupportedDecisionAnswer unsupported = Assert.IsType<UnsupportedDecisionAnswer>(result.Answers[4]);
            Assert.Equal("ranking", unsupported.OriginalType);
            Assert.Equal("future", unsupported.Name);
            Assert.Equal(0.02, result.Cost!.Amount);
        }

        [Fact]
        public async Task Decisions_Evaluate_SendsItemsInputAsArrayWithImagesAndFunctionCalls()
        {
            CapturingHandler handler = new("""{"instanceId":"11111111-1111-1111-1111-111111111111","answers":[]}""");

            await CreateClient(handler).GetDecisionClient().EvaluateAsync(new DecisionReq
            {
                ModelCode = "decision-model",
                Vendor = "OpenAI",
                Input = DecisionInput.FromItems(
                    DecisionInputItem.CreateMessage(DecisionMessageRole.User, DecisionContent.FromParts(
                        DecisionInputPart.CreateInputText("Is this receipt paid?"),
                        DecisionInputPart.CreateInputImage(new byte[] { 1, 2, 3 }, "image/png", DecisionImageDetail.Low))),
                    DecisionInputItem.CreateFunctionCall("call_1", "get_order", """{"id":42}"""),
                    DecisionInputItem.CreateFunctionCallOutput("call_1", "Order 42 is paid."),
                    DecisionInputItem.CreateMessage(DecisionMessageRole.Assistant, "Yes, it is paid.")),
                Questions = { new DecisionQuestion { Type = DecisionQuestionType.Predicate, Name = "paid", Instructions = "Is it paid?" } }
            });

            JsonElement input = handler.JsonBody!.Value.GetProperty("input");
            Assert.Equal(JsonValueKind.Array, input.ValueKind);
            Assert.Equal(4, input.GetArrayLength());

            JsonElement userMessage = input[0];
            Assert.Equal("message", userMessage.GetProperty("type").GetString());
            Assert.Equal("user", userMessage.GetProperty("role").GetString());
            JsonElement parts = userMessage.GetProperty("content");
            Assert.Equal("input_text", parts[0].GetProperty("type").GetString());
            Assert.Equal("Is this receipt paid?", parts[0].GetProperty("text").GetString());
            Assert.Equal("input_image", parts[1].GetProperty("type").GetString());
            Assert.Equal("data:image/png;base64,AQID", parts[1].GetProperty("imageUrl").GetString());
            Assert.Equal("low", parts[1].GetProperty("detail").GetString());
            Assert.False(parts[1].TryGetProperty("text", out _));

            Assert.Equal("function_call", input[1].GetProperty("type").GetString());
            Assert.Equal("call_1", input[1].GetProperty("callId").GetString());
            Assert.Equal("get_order", input[1].GetProperty("name").GetString());
            Assert.Equal("""{"id":42}""", input[1].GetProperty("arguments").GetString());

            Assert.Equal("function_call_output", input[2].GetProperty("type").GetString());
            Assert.Equal("Order 42 is paid.", input[2].GetProperty("output").GetString());

            Assert.Equal("assistant", input[3].GetProperty("role").GetString());
            Assert.Equal("Yes, it is paid.", input[3].GetProperty("content").GetString());
        }

        [Fact]
        public async Task Decisions_InvalidInput_ThrowsBeforeSending()
        {
            CapturingHandler handler = new("{}");
            IDecisionClient client = CreateClient(handler).GetDecisionClient();
            List<DecisionQuestion> questions = new() { new DecisionQuestion { Type = DecisionQuestionType.Predicate, Name = "q", Instructions = "?" } };

            await Assert.ThrowsAsync<ArgumentNullException>(() => client.EvaluateAsync(new DecisionReq { AIModelId = Guid.NewGuid(), Questions = questions }));
            await Assert.ThrowsAsync<ArgumentException>(() => client.EvaluateAsync(new DecisionReq { AIModelId = Guid.NewGuid(), Input = "", Questions = questions }));
            await Assert.ThrowsAsync<ArgumentException>(() => client.EvaluateAsync(new DecisionReq { AIModelId = Guid.NewGuid(), Input = DecisionInput.FromItems(), Questions = questions }));
            Assert.Equal(0, handler.RequestCount);
        }
        #endregion

        #region Embeddings
        [Fact]
        public async Task Embeddings_Generate_SendsContentAndParsesVectors()
        {
            CapturingHandler handler = new("""{"data":[{"index":0,"embedding":[0.1,-0.2,0.3]}],"cost":{"total":0.001,"currency":"USD"},"tokenUsage":{"totalTokens":3},"usage":{"inputUnit":"tokens","input":3}}""");
            Guid modelId = Guid.NewGuid();

            EmbeddingsResult result = await CreateClient(handler).GetEmbeddingClient().GenerateAsync(new GenerateEmbeddingsReq { AIModelId = modelId, Content = { "hello" } });

            AssertPost(handler, "/api/v2/embeddings/generate");
            Assert.Equal("hello", handler.JsonBody!.Value.GetProperty("content")[0].GetString());
            Assert.Equal(modelId, handler.JsonBody!.Value.GetProperty("modelId").GetGuid());
            Assert.Equal(new[] { 0.1f, -0.2f, 0.3f }, result.Data[0].Embedding);
            Assert.Equal("tokens", result.Usage!.InputUnit);
            Assert.Equal(3, result.Usage.Input);
            Assert.Equal(0.001, result.Cost!.Total);
        }
        #endregion

        #region Image generation
        [Fact]
        public async Task ImageGeneration_Generate_SendsJsonAndParsesResult()
        {
            CapturingHandler handler = new("""{"instanceId":"11111111-1111-1111-1111-111111111111","imageUrl":"https://files/img.png","usage":{"outputUnit":"images","output":1}}""");

            ImageGenerationResult result = await CreateClient(handler).GetImageGenerationClient().GenerateAsync(new GenerateImageReq { Prompt = "A cat" });

            AssertPost(handler, "/api/v2/image/generate");
            Assert.Equal("application/json", handler.ContentMediaType);
            Assert.Equal("A cat", handler.JsonBody!.Value.GetProperty("prompt").GetString());
            Assert.Equal("https://files/img.png", result.ImageUrl);
            Assert.Equal(1, result.Usage!.Output);
        }

        [Fact]
        public async Task ImageGeneration_GenerateByFileId_SendsFileIds()
        {
            CapturingHandler handler = new("""{"instanceId":"11111111-1111-1111-1111-111111111111","imageUrl":"u"}""");
            Guid fileId = Guid.NewGuid();
            Guid modelId = Guid.NewGuid();

            await CreateClient(handler).GetImageGenerationClient().GenerateByFileIdAsync(new GenerateImageByFileIdReq { AIModelId = modelId, Prompt = "Edit", FileIds = { fileId } });

            AssertPost(handler, "/api/v2/image/generate/file");
            Assert.Equal(fileId, handler.JsonBody!.Value.GetProperty("fileIds")[0].GetGuid());
            Assert.Equal(modelId, handler.JsonBody!.Value.GetProperty("modelId").GetGuid());
        }

        [Fact]
        public async Task ImageGeneration_GenerateWithSourceImages_SendsMultipartOnGenerateRoute()
        {
            CapturingHandler handler = new("""{"instanceId":"11111111-1111-1111-1111-111111111111","imageUrl":"https://files/img.png"}""");
            Guid modelId = Guid.NewGuid();

            ImageGenerationResult result = await CreateClient(handler).GetImageGenerationClient().GenerateWithSourceImagesAsync(new GenerateImageWithSourceImagesReq
            {
                AIModelId = modelId,
                Prompt = "Combine them",
                Files =
                {
                    new AIServiceFile { FileStream = FileStreamOf("a"), FileName = "a.png" },
                    new AIServiceFile { FileStream = FileStreamOf("b"), FileName = "b.webp" }
                }
            });

            AssertPost(handler, "/api/v2/image/generate");
            Assert.Equal("multipart/form-data", handler.ContentMediaType);
            Assert.Contains(handler.FormParts, p => p.Name == "Files" && p.FileName == "a.png" && p.MediaType == "image/png");
            Assert.Contains(handler.FormParts, p => p.Name == "Files" && p.FileName == "b.webp" && p.MediaType == "image/webp");
            Assert.Equal(modelId.ToString(), handler.FormValue("ModelId"));
            Assert.Equal("Combine them", handler.FormValue("Prompt"));
            Assert.Equal("https://files/img.png", result.ImageUrl);
        }

        [Fact]
        public async Task ImageGeneration_GenerateWithSourceImages_RequiresAtLeastOneFile()
        {
            CapturingHandler handler = new("{}");

            await Assert.ThrowsAsync<ArgumentException>(() => CreateClient(handler).GetImageGenerationClient()
                .GenerateWithSourceImagesAsync(new GenerateImageWithSourceImagesReq { Prompt = "Edit" }));
            Assert.Equal(0, handler.RequestCount);
        }

        [Fact]
        public async Task ImageGeneration_RejectsBlankPrompt()
        {
            CapturingHandler handler = new("{}");
            IImageGenerationClient client = CreateClient(handler).GetImageGenerationClient();

            await Assert.ThrowsAsync<ArgumentNullException>(() => client.GenerateAsync(new GenerateImageReq { Prompt = "   " }));
            await Assert.ThrowsAsync<ArgumentNullException>(() => client.GenerateByFileIdAsync(new GenerateImageByFileIdReq { Prompt = "", FileIds = { Guid.NewGuid() } }));
            Assert.Equal(0, handler.RequestCount);
        }
        #endregion

        #region OCR
        private const string OcrResponse = """
            {"instanceId":"11111111-1111-1111-1111-111111111111","files":[
              {"index":0,"source":"a.pdf","success":true,"result":{"model":"ocr","provider":"Mistral","markdown":"# Title","usage":{"inputTokenCount":5,"additionalCounts":{"pages":1}},
                "pages":[{"index":0,"markdown":"# Title","tables":[{"content":"<table/>","format":0,"boundingBox":{"x":0.1,"y":0.2,"width":0.3,"height":0.4}}],"dimensions":{"width":800,"height":600}}]}},
              {"index":1,"source":"b.png","success":false,"error":"Unreadable"}],
             "usage":{"inputUnit":"pages","input":1},"cost":{"amount":0.01,"currency":"USD"}}
            """;

        [Fact]
        public async Task Ocr_Extract_SendsEveryFileAndParsesResult()
        {
            CapturingHandler handler = new(OcrResponse);

            OcrExecutionResult result = await CreateClient(handler).GetOcrClient().ExtractAsync(new OcrReq
            {
                Files =
                {
                    new AIServiceFile { FileStream = FileStreamOf("pdf"), FileName = "a.pdf" },
                    new AIServiceFile { FileStream = FileStreamOf("png"), FileName = "b.png" }
                },
                Instructions = "Extract tables"
            });

            AssertPost(handler, "/api/v2/ocr");
            Assert.Equal(2, handler.FormParts.Count(p => p.Name == "Files"));
            Assert.Contains(handler.FormParts, p => p.Name == "Files" && p.FileName == "a.pdf" && p.MediaType == "application/pdf");
            Assert.Equal("Extract tables", handler.FormValue("Instructions"));

            OcrFileResult first = result.Files[0];
            Assert.True(first.Success);
            Assert.Equal("# Title", first.Result!.Markdown);
            Assert.Equal(1, first.Result.Usage!.AdditionalCounts!["pages"]);
            OcrTable table = first.Result.Pages![0].Tables[0];
            Assert.Equal(OcrTableFormat.Html, table.Format);
            Assert.Equal(0.4, table.BoundingBox!.Height);
            Assert.Equal(800, first.Result.Pages[0].Dimensions!.Width);

            Assert.False(result.Files[1].Success);
            Assert.Equal("Unreadable", result.Files[1].Error);
            Assert.Equal("pages", result.Usage!.InputUnit);
            Assert.Equal(1, result.Usage.Input);
            Assert.Equal(0.01, result.Cost!.Amount);
        }

        [Fact]
        public async Task Ocr_JsonVariants_UseTheirRoutesAndDerivedFields()
        {
            CapturingHandler base64Handler = new(OcrResponse);
            await CreateClient(base64Handler).GetOcrClient().ExtractFromBase64Async(new OcrBase64Req
            {
                Base64Files = { new OcrBase64File { ContentBase64 = "AAAA", MimeType = "application/pdf" } },
                Instructions = "x"
            });
            AssertPost(base64Handler, "/api/v2/ocr/base64");
            Assert.Equal("AAAA", base64Handler.JsonBody!.Value.GetProperty("base64Files")[0].GetProperty("contentBase64").GetString());
            Assert.Equal("x", base64Handler.JsonBody.Value.GetProperty("instructions").GetString());

            CapturingHandler urlHandler = new(OcrResponse);
            await CreateClient(urlHandler).GetOcrClient().ExtractFromUrlsAsync(new OcrUrlReq { Urls = { "https://example.com/a.pdf" } });
            AssertPost(urlHandler, "/api/v2/ocr/url");
            Assert.Equal("https://example.com/a.pdf", urlHandler.JsonBody!.Value.GetProperty("urls")[0].GetString());

            CapturingHandler fileIdHandler = new(OcrResponse);
            Guid fileId = Guid.NewGuid();
            await CreateClient(fileIdHandler).GetOcrClient().ExtractByFileIdAsync(new OcrByFileIdReq { FileIds = { fileId } });
            AssertPost(fileIdHandler, "/api/v2/ocr/file");
            Assert.Equal(fileId, fileIdHandler.JsonBody!.Value.GetProperty("fileIds")[0].GetGuid());
        }

        [Fact]
        public async Task Ocr_InvalidRequests_ThrowBeforeSending()
        {
            CapturingHandler handler = new(OcrResponse);
            IOcrClient ocr = CreateClient(handler).GetOcrClient();

            await Assert.ThrowsAsync<ArgumentException>(() => ocr.ExtractAsync(new OcrReq()));
            await Assert.ThrowsAsync<ArgumentException>(() => ocr.ExtractFromUrlsAsync(new OcrUrlReq { Urls = { "file:///etc/passwd" } }));
            await Assert.ThrowsAsync<ArgumentException>(() => ocr.ExtractByFileIdAsync(new OcrByFileIdReq { FileIds = { Guid.Empty } }));
            await Assert.ThrowsAsync<ArgumentException>(() => ocr.ExtractFromBase64Async(new OcrBase64Req { Base64Files = { new OcrBase64File { ContentBase64 = "   ", MimeType = "application/pdf" } } }));
            await Assert.ThrowsAsync<ArgumentException>(() => ocr.ExtractFromUrlsAsync(new OcrUrlReq { Urls = { "" } }));
            Assert.Equal(0, handler.RequestCount);
        }
        #endregion

        #region Cross-cutting
        [Fact]
        public async Task ErrorResponse_ThrowsSerenityApiException_WithPayload()
        {
            CapturingHandler handler = new("""{"message":"Vendor not found","code":"not_found"}""", statusCode: HttpStatusCode.NotFound);

            SerenityApiException exception = await Assert.ThrowsAsync<SerenityApiException>(() =>
                CreateClient(handler).GetAnonymizerClient().ObfuscateAsync(new AnonymizerReq { Inputs = "x" }));

            Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
            Assert.Equal("not_found", exception.ResponseJson!.Value.GetProperty("code").GetString());
        }

        [Fact]
        public void GetClients_ThrowArgumentNullException_ForNullClient()
        {
            SerenityClient client = null!;

            Assert.Throws<ArgumentNullException>(() => client.GetAnonymizerClient());
            Assert.Throws<ArgumentNullException>(() => client.GetDecisionClient());
            Assert.Throws<ArgumentNullException>(() => client.GetEmbeddingClient());
            Assert.Throws<ArgumentNullException>(() => client.GetImageGenerationClient());
            Assert.Throws<ArgumentNullException>(() => client.GetOcrClient());
        }

        [Fact]
        public async Task DependencyInjection_RegistersEveryAIService()
        {
            CapturingHandler handler = new("""{"text":"ok","spans":[]}""");

            ServiceCollection services = new();
            services.AddSerenityStar("di-key", baseUrl: BaseUrl)
                    .AddSerenityStarAIServices();
            services.AddHttpClient<SerenityClient>()
                    .ConfigurePrimaryHttpMessageHandler(() => handler);

            using ServiceProvider provider = services.BuildServiceProvider();

            Assert.NotNull(provider.GetRequiredService<IDecisionClient>());
            Assert.NotNull(provider.GetRequiredService<IEmbeddingClient>());
            Assert.NotNull(provider.GetRequiredService<IImageGenerationClient>());
            Assert.NotNull(provider.GetRequiredService<IOcrClient>());

            await provider.GetRequiredService<IAnonymizerClient>().ObfuscateAsync(new AnonymizerReq { Inputs = "x" });

            Assert.Equal(new[] { "di-key" }, handler.ApiKeyValues);
            Assert.Equal($"{BaseUrl}/api/v2/anonymizer/obfuscate", handler.RequestUri!.AbsoluteUri);
        }
        #endregion
    }
}
