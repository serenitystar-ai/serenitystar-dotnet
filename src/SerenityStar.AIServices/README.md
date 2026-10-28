# SubgenAI.SerenityStar.AIServices

Serenity Star AI services for .NET: anonymizer, decisions, embeddings, image generation and OCR.

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

## Dependency injection

```csharp
using SerenityStar.Extensions;

services.AddSerenityStar("your-api-key")
        .AddSerenityStarAIServices();

// Then inject any AI service interface, e.g. IAnonymizerClient.
```

See the full documentation and examples at https://github.com/serenitystar-ai/serenitystar-dotnet.
