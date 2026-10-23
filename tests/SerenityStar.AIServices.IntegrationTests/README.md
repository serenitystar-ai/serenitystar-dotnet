# AI Services Integration Tests

This project has two kinds of tests:

- **`AIServicesOfflineTests`**: run without an API key or network. A fake HTTP handler captures each request and returns a canned response, to check the requests the SDK builds and how it parses responses.
- **`*IntegrationTests`**: run against the real Serenity Star API. They consume credits from the tenant of the configured API key.

## Configuration

Create an `appsettings.Development.json` file in this directory (it is git-ignored) and set every value marked `replace-me` in `appsettings.json`. Empty values and `replace-me` are treated as not configured; a test that needs a missing value fails with a message naming the setting.

Models are selected by code and vendor (`*ModelCode` + `*Vendor`) on the services that support it: decisions, OCR, speech generation and vision. Both values must be set together. The vendor is the name of a Serenity Star vendor, e.g. `OpenAI`, `Anthropic`, `Google` or `ElevenLabs`. Embeddings, image generation and transcription only accept a model id (`*ModelId`).

```json
{
  "SerenityStar": {
    "ApiKey": "your-api-key",
    "AudioFilePath": "C:\test-files\audio.mp3",
    "DecisionModelCode": "your-decision-model-code",
    "DecisionVendor": "OpenAI",
    "ImageFilePath": "C:\test-files\image.png",
    "ImageGenerationWithSourceImagesModelId": "00000000-0000-0000-0000-000000000000",
    "OcrFilePath": "C:\test-files\document.pdf",
    "OcrUrl": "https://example.com/document.pdf",
    "SpeechGenerationModelCode": "tts-1",
    "SpeechGenerationVendor": "OpenAI"
  }
}
```

| Setting | Required | Description |
| --- | --- | --- |
| `ApiKey` | Yes | API key with permission to execute AI services. |
| `AudioFilePath` | Yes | Audio file for transcription (mp3, mp4, mpeg, mpga, m4a, wav, webm; max 25 MB). |
| `BaseUrl` | No | API URL. Leave empty to run against production (`https://api.serenitystar.ai`). |
| `DecisionModelCode` | Yes | Code of a decision model. |
| `DecisionVendor` | Yes | Vendor of the decision model. |
| `EmbeddingModelId` | No | Embedding model id. Uses the default model when empty. |
| `ImageFilePath` | Yes | Image for vision and image generation from source images (jpg, jpeg, png, gif, webp; max 20 MB). |
| `ImageGenerationModelId` | No | Image generation model id for prompt-only generation. Uses the default model when empty. |
| `ImageGenerationWithSourceImagesModelId` | Yes | Id of an image generation model that accepts source images. |
| `OcrFilePath` | Yes | Document with text for OCR (pdf, png, jpg, jpeg; max 50 MB). |
| `OcrModelCode` | No | Code of an OCR model. Uses the default model when empty. |
| `OcrUrl` | Yes | Public http(s) URL of a document with text for OCR. |
| `OcrVendor` | No | Vendor of the OCR model. Required when `OcrModelCode` is set. |
| `SpeechGenerationModelCode` | Yes | Code of a speech generation (text-to-speech) model. |
| `SpeechGenerationVendor` | Yes | Vendor of the speech generation model. |
| `SpeechGenerationVoice` | No | Voice name. Uses the vendor's default voice when empty. |
| `TranscriptionModelId` | No | Transcription model id. Uses the default model when empty. |
| `VisionModelCode` | No | Code of a vision model. Uses the default model when empty. |
| `VisionVendor` | No | Vendor of the vision model. Required when `VisionModelCode` is set. |

Settings can also be provided as environment variables, e.g. `SerenityStar__ApiKey`.

The `*ByFileIdAsync` tests upload the configured file to Serenity Star as a **private** file first. Use non-sensitive test files only: they are sent to the configured AI providers.
