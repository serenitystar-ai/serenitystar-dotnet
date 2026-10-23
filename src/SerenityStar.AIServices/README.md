# SubgenAI.SerenityStar.AIServices

Serenity Star AI services for .NET: transcription, speech, image generation, vision, OCR, embeddings, anonymizer and decisions.

This package depends only on `SubgenAI.SerenityStar.Core`, so you don't need the agents package (`SubgenAI.SerenityStar.SDK`). All Serenity Star packages are published together and must be used with the same version.

```
dotnet add package SubgenAI.SerenityStar.AIServices
```

## Usage

Create a single `SerenityClient` and get one client per AI service from it:

```csharp
using SerenityStar.AIServices;
using SerenityStar.Client;
using SerenityStar.Models.Transcription;

SerenityClient client = SerenityClient.Create("your-api-key");

ITranscriptionClient transcription = client.GetTranscriptionClient();

using FileStream audioFile = File.OpenRead("meeting.mp3");
TranscribeResult result = await transcription.TranscribeAsync(new TranscribeAudioReq
{
    FileStream = audioFile,
    FileName = "meeting.mp3"
});
```

| Service | Interface | Entry point |
| --- | --- | --- |
| Transcription | `ITranscriptionClient` | `client.GetTranscriptionClient()` |
| Speech generation (text-to-speech) | `ISpeechGenerationClient` | `client.GetSpeechGenerationClient()` |
| Image generation | `IImageGenerationClient` | `client.GetImageGenerationClient()` |
| Vision | `IVisionClient` | `client.GetVisionClient()` |
| OCR | `IOcrClient` | `client.GetOcrClient()` |
| Embeddings | `IEmbeddingClient` | `client.GetEmbeddingClient()` |
| Anonymizer | `IAnonymizerClient` | `client.GetAnonymizerClient()` |
| Decisions | `IDecisionClient` | `client.GetDecisionClient()` |

## Dependency injection

```csharp
using SerenityStar.Extensions;

services.AddSerenityStar("your-api-key")
        .AddSerenityStarAIServices();

// Then inject any interface, e.g. ITranscriptionClient or IOcrClient.
```

See the full documentation and examples at https://github.com/serenitystar-ai/serenitystar-dotnet.
