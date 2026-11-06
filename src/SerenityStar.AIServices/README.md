# SubgenAI.SerenityStar.AIServices

Serenity Star AI services for .NET: anonymizer, decisions, embeddings, image generation, OCR, speech generation, transcription and vision.

This package depends only on `SubgenAI.SerenityStar.Core`, so you don't need the agents package (`SubgenAI.SerenityStar.SDK`). All Serenity Star packages are published together and must be used with the same version.

```
dotnet add package SubgenAI.SerenityStar.AIServices
```

## Usage

Create a single `SerenityClient` and get one client per AI service from it:

```csharp
using SerenityStar.AIServices;
using SerenityStar.Client;
using SerenityStar.Models.Anonymizer;

SerenityClient client = SerenityClient.Create("your-api-key");

IAnonymizerClient anonymizer = client.GetAnonymizerClient();

AnonymizerResult result = await anonymizer.ObfuscateAsync(new AnonymizerReq
{
    Inputs = "My name is Jane Doe and my email is jane.doe@example.com"
});
```

| Service | Interface | Entry point |
| --- | --- | --- |
| Anonymizer | `IAnonymizerClient` | `client.GetAnonymizerClient()` |
| Decisions | `IDecisionClient` | `client.GetDecisionClient()` |
| Embeddings | `IEmbeddingClient` | `client.GetEmbeddingClient()` |
| Image generation | `IImageGenerationClient` | `client.GetImageGenerationClient()` |
| OCR | `IOcrClient` | `client.GetOcrClient()` |
| Speech generation (text-to-speech) | `ISpeechGenerationClient` | `client.GetSpeechGenerationClient()` |
| Transcription | `ITranscriptionClient` | `client.GetTranscriptionClient()` |
| Vision | `IVisionClient` | `client.GetVisionClient()` |

Services that work with files also accept the id of a file uploaded beforehand (`*ByFileIdAsync` methods). Upload it with `client.GetFileClient().UploadAsync(...)`, which returns the file id.

## Dependency injection

```csharp
using SerenityStar.Extensions;

services.AddSerenityStar("your-api-key")
        .AddSerenityStarAIServices();

// Then inject any interface, e.g. ITranscriptionClient or IOcrClient.
```

## Error handling

Failed requests throw `SerenityApiException`. When the AI provider fails, its `Error` is an `AIServiceExecutionFailedError` listing every attempt made (`Attempts`), or, for OCR, the attempts made for each file (`Files`). Wait `Error.RetryAfter` before retrying when it's set.

```csharp
using SerenityStar.Errors;
using SerenityStar.Errors.Models;

try
{
    TranscriptionResult result = await client.GetTranscriptionClient().TranscribeAsync(request);
}
catch (SerenityApiException ex) when (ex.Error is AIServiceExecutionFailedError failed)
{
    foreach (ExecutionAttempt attempt in failed.Attempts ?? new List<ExecutionAttempt>())
        Console.WriteLine($"{attempt.Code} (HTTP {attempt.StatusCode}): {attempt.VendorError}");
}
catch (SerenityApiException ex)
{
    Console.WriteLine($"{ex.Error.Code}: {ex.Message}");
}
```

A speech generation stream throws `SerenityApiException` only when the request is rejected before the stream opens. After that, failures arrive as a `StreamingSpeechGenerationMessageError` with the same `Error`:

```csharp
using SerenityStar.Models.SpeechGeneration;

await foreach (StreamingSpeechGenerationMessage message in client.GetSpeechGenerationClient().StreamAsync(request))
{
    switch (message)
    {
        case StreamingSpeechGenerationMessageAudio audio:
            byte[] bytes = audio.GetAudioBytes();
            break;
        case StreamingSpeechGenerationMessageError errorMessage:
            Console.WriteLine($"{errorMessage.Error.Code}: {errorMessage.Message}");
            break;
    }
}
```

See [Error handling](https://github.com/serenitystar-ai/serenitystar-dotnet#error-handling) for the full error model.

See the full documentation and examples at https://github.com/serenitystar-ai/serenitystar-dotnet.
