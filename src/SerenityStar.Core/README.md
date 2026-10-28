# SubgenAI.SerenityStar.Core

Core package of the Serenity Star .NET SDK. It contains `SerenityClient` and everything needed to talk to the Serenity Star API: authentication, error handling (`SerenityApiException`) and file upload.

You normally don't install this package directly. Install the feature packages you need; they bring this one in with the same version:

| Package | Features |
| --- | --- |
| `SubgenAI.SerenityStar.SDK` | Agents: assistants, copilots, activities, AI proxies and chat completions |
| `SubgenAI.SerenityStar.AIServices` | AI services: anonymizer, decisions, embeddings and image generation |

All packages are always published together and must be used with the same version.

```csharp
using SerenityStar.Client;

SerenityClient client = SerenityClient.Create("your-api-key");

// Each feature package adds extension methods to SerenityClient, e.g.
// client.GetAgents()               (SubgenAI.SerenityStar.SDK)
// client.GetAnonymizerClient()     (SubgenAI.SerenityStar.AIServices)
```

See the full documentation at https://github.com/serenitystar-ai/serenitystar-dotnet.
