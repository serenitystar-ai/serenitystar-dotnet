# SubgenAI.SerenityStar.Core

Core package of the Serenity Star .NET SDK. It contains `SerenityClient` and everything needed to talk to the Serenity Star API: authentication, error handling (`SerenityApiException`) and file upload.

You normally don't install this package directly. Install the feature packages you need; they bring this one in with the same version:

| Package | Features |
| --- | --- |
| `SubgenAI.SerenityStar.SDK` | Agents: assistants, copilots, activities, AI proxies and chat completions |
| `SubgenAI.SerenityStar.AIServices` | AI services: anonymizer, decisions, embeddings, image generation, OCR, speech generation and transcription |

All packages are always published together and must be used with the same version.

```csharp
using SerenityStar.Client;

SerenityClient client = SerenityClient.Create("your-api-key");

// Each feature package adds extension methods to SerenityClient, e.g.
// client.GetAgents()               (SubgenAI.SerenityStar.SDK)
// client.GetAnonymizerClient()     (SubgenAI.SerenityStar.AIServices)
```

## File upload

Upload a file once and use its id later, e.g. in the AI services `*ByFileIdAsync` methods or in volatile knowledge `UploadFromFileForAgentAsync`:

```csharp
using SerenityStar.Files;
using SerenityStar.Models.Files;

using FileStream fileStream = File.OpenRead("audio.mp3");
UploadFileResult file = await client.GetFileClient().UploadAsync(new UploadFileReq
{
    FileStream = fileStream,
    FileName = "audio.mp3"
});

// Or from a public URL
UploadFileResult fromUrl = await client.GetFileClient().UploadFromUrlAsync(new UploadFileFromUrlReq
{
    FileUrl = "https://example.com/invoice.pdf"
});
```

`AddSerenityStar` registers `IFileClient` for dependency injection.

See the full documentation at https://github.com/serenitystar-ai/serenitystar-dotnet.
