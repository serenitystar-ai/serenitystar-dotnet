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
using SerenityStar.Models.SpeechGeneration;
using SerenityStar.Models.Transcription;
using SerenityStar.Models.Vision;
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
                    DecisionInputItem.Message(DecisionMessageRole.User, DecisionContent.FromParts(
                        DecisionInputPart.InputText("Is this receipt paid?"),
                        DecisionInputPart.InputImage(new byte[] { 1, 2, 3 }, "image/png", DecisionImageDetail.Low))),
                    DecisionInputItem.FunctionCall("call_1", "get_order", """{"id":42}"""),
                    DecisionInputItem.FunctionCallOutput("call_1", "Order 42 is paid."),
                    DecisionInputItem.Message(DecisionMessageRole.Assistant, "Yes, it is paid.")),
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
            CapturingHandler handler = new("""{"data":[{"index":0,"embedding":[0.1,-0.2,0.3]}],"cost":{"total":0.001,"currency":"USD"},"tokenUsage":{"totalTokens":3}}""");

            EmbeddingsResult result = await CreateClient(handler).GetEmbeddingClient().GenerateAsync(new GenerateEmbeddingsReq { Content = { "hello" } });

            AssertPost(handler, "/api/v2/embeddings/generate");
            Assert.Equal("hello", handler.JsonBody!.Value.GetProperty("content")[0].GetString());
            Assert.Equal(new[] { 0.1f, -0.2f, 0.3f }, result.Data[0].Embedding);
            Assert.Equal(3, result.TokenUsage!.TotalTokens);
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

            await CreateClient(handler).GetImageGenerationClient().GenerateByFileIdAsync(new GenerateImageByFileIdReq { Prompt = "Edit", FileIds = { fileId } });

            AssertPost(handler, "/api/v2/image/generate/file");
            Assert.Equal(fileId, handler.JsonBody!.Value.GetProperty("fileIds")[0].GetGuid());
        }

        [Fact]
        public async Task ImageGeneration_GenerateWithSourceImages_SendsMultipartOnGenerateRoute()
        {
            CapturingHandler handler = new("""{"instanceId":"11111111-1111-1111-1111-111111111111","imageUrl":"https://files/img.png"}""");
            Guid modelId = Guid.NewGuid();

            ImageGenerationResult result = await CreateClient(handler).GetImageGenerationClient().GenerateWithSourceImagesAsync(new GenerateImageWithSourceImagesReq
            {
                ModelId = modelId,
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
            Assert.Equal(0, handler.RequestCount);
        }
        #endregion

        #region Speech generation
        [Fact]
        public async Task SpeechGeneration_Generate_SendsJsonWithStreamFalse()
        {
            CapturingHandler handler = new("""{"instanceId":"11111111-1111-1111-1111-111111111111","audioUrl":"https://files/audio.mp3","cost":{"amount":0.5,"currency":"USD"}}""");
            Guid modelId = Guid.NewGuid();

            SpeechGenerationResult result = await CreateClient(handler).GetSpeechGenerationClient().GenerateAsync(new GenerateSpeechReq
            {
                AIModelId = modelId,
                Text = "Hello",
                Speed = 1.5f
            });

            AssertPost(handler, "/api/v2/audio/generate");
            JsonElement body = handler.JsonBody!.Value;
            Assert.Equal(modelId, body.GetProperty("aiModelId").GetGuid());
            Assert.Equal("Hello", body.GetProperty("text").GetString());
            Assert.Equal(1.5, body.GetProperty("speed").GetDouble());
            Assert.False(body.GetProperty("stream").GetBoolean());
            Assert.False(body.TryGetProperty("voice", out _));

            Assert.Equal("https://files/audio.mp3", result.AudioUrl);
            Assert.Equal(0.5, result.Cost!.Amount);
        }

        [Fact]
        public async Task SpeechGeneration_Stream_ParsesEveryEventType()
        {
            string sse = string.Join("\n",
                "event: start",
                """data: {"type":"start","instance_id":"11111111-1111-1111-1111-111111111111","model":"tts-1","mime_type":"audio/mpeg"}""",
                "",
                "event: audio",
                $$"""data: {"type":"audio","index":0,"data":"{{Convert.ToBase64String(new byte[] { 1, 2, 3 })}}"}""",
                "",
                """data: {"type":"ping"}""",
                "",
                """data: {"type":"something_new","foo":1}""",
                "",
                "event: stop",
                """data: {"type":"stop","instance_id":"11111111-1111-1111-1111-111111111111","audio_url":"https://files/a.mp3","usage":{"inputUnit":"characters","input":5},"cost":{"amount":0.1,"currency":"USD"}}""",
                "");
            CapturingHandler handler = new(sse, "text/event-stream");

            List<StreamingSpeechGenerationMessage> messages = new();
            await foreach (StreamingSpeechGenerationMessage message in CreateClient(handler).GetSpeechGenerationClient().StreamAsync(new GenerateSpeechReq { ModelCode = "tts-1", Vendor = "OpenAI", Text = "Hello" }))
                messages.Add(message);

            AssertPost(handler, "/api/v2/audio/generate");
            Assert.True(handler.JsonBody!.Value.GetProperty("stream").GetBoolean());

            Assert.Collection(messages,
                m => Assert.Equal("audio/mpeg", Assert.IsType<StreamingSpeechGenerationMessageStart>(m).MimeType),
                m => Assert.Equal(new byte[] { 1, 2, 3 }, Assert.IsType<StreamingSpeechGenerationMessageAudio>(m).GetAudioBytes()),
                m => Assert.IsType<StreamingSpeechGenerationMessagePing>(m),
                m => Assert.Equal("something_new", Assert.IsType<StreamingSpeechGenerationMessageUnsupported>(m).OriginalType),
                m =>
                {
                    StreamingSpeechGenerationMessageStop stop = Assert.IsType<StreamingSpeechGenerationMessageStop>(m);
                    Assert.Equal("https://files/a.mp3", stop.AudioUrl);
                    Assert.Equal("characters", stop.Usage!.InputUnit);
                    Assert.Equal(0.1, stop.Cost!.Amount);
                });
        }

        [Fact]
        public async Task SpeechGeneration_Stream_ThrowsSerenityApiException_WhenRejectedBeforeStreaming()
        {
            CapturingHandler handler = new("""{"message":"Validation failed"}""", statusCode: HttpStatusCode.BadRequest);

            await Assert.ThrowsAsync<SerenityApiException>(async () =>
            {
                await foreach (StreamingSpeechGenerationMessage _ in CreateClient(handler).GetSpeechGenerationClient().StreamAsync(new GenerateSpeechReq { Text = "Hello" }))
                {
                }
            });
        }

        [Fact]
        public void SpeechGeneration_Stream_ValidatesEagerly()
        {
            CapturingHandler handler = new("{}");

            Assert.Throws<ArgumentNullException>(() => CreateClient(handler).GetSpeechGenerationClient().StreamAsync(new GenerateSpeechReq()));
            Assert.Equal(0, handler.RequestCount);
        }
        #endregion

        #region Transcription
        [Fact]
        public async Task Transcription_Transcribe_SendsMultipartAndParsesResult()
        {
            CapturingHandler handler = new(
                """{"instanceId":"11111111-1111-1111-1111-111111111111","transcript":"hello","metadata":{"language":"en","duration":"00:00:05"},"usage":{"inputUnit":"seconds","input":5},"cost":{"prompt":0.1,"completion":0.2,"total":0.3,"currency":"USD"}}""");
            Guid modelId = Guid.NewGuid();

            TranscribeResult result = await CreateClient(handler).GetTranscriptionClient().TranscribeAsync(new TranscribeAudioReq
            {
                FileStream = FileStreamOf("audio"),
                FileName = "meeting.mp3",
                ModelId = modelId,
                Prompt = "context",
                Channel = "web"
            });

            AssertPost(handler, "/api/v2/audio/transcribe");
            Assert.Equal("multipart/form-data", handler.ContentMediaType);
            Assert.Contains(handler.FormParts, p => p.Name == "File" && p.FileName == "meeting.mp3" && p.MediaType == "audio/mpeg");
            Assert.Equal(modelId.ToString(), handler.FormValue("ModelId"));
            Assert.Equal("context", handler.FormValue("Prompt"));
            Assert.Equal("web", handler.FormValue("Channel"));
            Assert.DoesNotContain(handler.FormParts, p => p.Name == "UserIdentifier");

            Assert.Equal("hello", result.Transcript);
            Assert.Equal(TimeSpan.FromSeconds(5), result.Metadata!.Duration);
            Assert.Equal("seconds", result.Usage!.InputUnit);
            Assert.Equal(0.3, result.Cost!.Total);
        }

        [Fact]
        public async Task Transcription_TranscribeByFileId_SendsJson()
        {
            CapturingHandler handler = new("""{"instanceId":"11111111-1111-1111-1111-111111111111","transcript":"hi"}""");
            Guid fileId = Guid.NewGuid();

            await CreateClient(handler).GetTranscriptionClient().TranscribeByFileIdAsync(new TranscribeAudioByFileIdReq { FileId = fileId });

            AssertPost(handler, "/api/v2/audio/transcribe/file");
            Assert.Equal(fileId, handler.JsonBody!.Value.GetProperty("fileId").GetGuid());
            // Nulls are omitted so the server applies its defaults.
            Assert.False(handler.JsonBody.Value.TryGetProperty("modelId", out _));
        }
        #endregion

        #region Vision
        [Fact]
        public async Task Vision_Analyze_SendsMultipartAndParsesResult()
        {
            CapturingHandler handler = new("""{"instanceId":"11111111-1111-1111-1111-111111111111","response":"A dog","tokenUsage":{"promptTokens":10,"completionTokens":2,"totalTokens":12},"cost":{"prompt":0.1,"completion":0.1,"total":0.2,"currency":"USD"}}""");

            VisionResult result = await CreateClient(handler).GetVisionClient().AnalyzeAsync(new AnalyzeImageReq
            {
                FileStream = FileStreamOf("img"),
                FileName = "photo.JPG",
                ModelCode = "gpt-4o",
                Vendor = "OpenAI",
                Prompt = "Describe"
            });

            AssertPost(handler, "/api/v2/image/vision");
            Assert.Contains(handler.FormParts, p => p.Name == "File" && p.FileName == "photo.JPG" && p.MediaType == "image/jpeg");
            Assert.Equal("gpt-4o", handler.FormValue("ModelCode"));
            Assert.Equal("OpenAI", handler.FormValue("Vendor"));
            Assert.Equal("Describe", handler.FormValue("Prompt"));
            Assert.DoesNotContain(handler.FormParts, p => p.Name == "AIModelId");

            Assert.Equal("A dog", result.Response);
            Assert.Equal(12, result.TokenUsage!.TotalTokens);
            Assert.Equal(0.2, result.Cost!.Total);
        }

        [Fact]
        public async Task Vision_AnalyzeByFileId_SendsJson()
        {
            CapturingHandler handler = new("""{"instanceId":"11111111-1111-1111-1111-111111111111","response":"ok"}""");
            Guid fileId = Guid.NewGuid();

            await CreateClient(handler).GetVisionClient().AnalyzeByFileIdAsync(new AnalyzeImageByFileIdReq { FileId = fileId });

            AssertPost(handler, "/api/v2/image/vision/file");
            Assert.Equal(fileId, handler.JsonBody!.Value.GetProperty("fileId").GetGuid());
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
            Assert.Throws<ArgumentNullException>(() => client.GetSpeechGenerationClient());
            Assert.Throws<ArgumentNullException>(() => client.GetTranscriptionClient());
            Assert.Throws<ArgumentNullException>(() => client.GetVisionClient());
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
            Assert.NotNull(provider.GetRequiredService<ISpeechGenerationClient>());
            Assert.NotNull(provider.GetRequiredService<ITranscriptionClient>());
            Assert.NotNull(provider.GetRequiredService<IVisionClient>());

            await provider.GetRequiredService<IAnonymizerClient>().ObfuscateAsync(new AnonymizerReq { Inputs = "x" });

            Assert.Equal(new[] { "di-key" }, handler.ApiKeyValues);
            Assert.Equal($"{BaseUrl}/api/v2/anonymizer/obfuscate", handler.RequestUri!.AbsoluteUri);
        }
        #endregion
    }
}
