# AI Services Integration Tests

This project has two kinds of tests:

- **`AIServicesOfflineTests`**: run without an API key or network. A fake HTTP handler captures each request and returns a canned response, to check the requests the SDK builds and how it parses responses.
- **`*IntegrationTests`**: run against the real Serenity Star API. They consume credits from the tenant of the configured API key.

## Configuration

Create an `appsettings.Development.json` file in this directory (it is git-ignored) and set every value marked `replace-me` in `appsettings.json`. Empty values and `replace-me` are treated as not configured; a test that needs a missing value fails with a message naming the setting.

Models are selected by code and vendor (`*ModelCode` + `*Vendor`) on the services that support it: decisions and OCR. Both values must be set together. The vendor is the name of a Serenity Star vendor, e.g. `OpenAI`, `Anthropic`, `Google` or `Mistral`. Embeddings and image generation only accept a model id (`*ModelId`).

```json
{
  "SerenityStar": {
    "ApiKey": "your-api-key",
    "DecisionModelCode": "your-decision-model-code",
    "DecisionVendor": "OpenAI",
    "ImageFilePath": "C:\\test-files\\image.png",
    "ImageGenerationWithSourceImagesModelId": "00000000-0000-0000-0000-000000000000",
    "OcrFilePath": "C:\\test-files\\document.pdf",
    "OcrUrl": "https://example.com/document.pdf"
  }
}
```

| Setting | Required | Description |
| --- | --- | --- |
| `ApiKey` | Yes | API key with permission to execute AI services. |
| `BaseUrl` | No | API URL. Leave empty to run against production (`https://api.serenitystar.ai`). |
| `DecisionModelCode` | Yes | Code of a decision model. |
| `DecisionVendor` | Yes | Vendor of the decision model. |
| `EmbeddingModelId` | No | Embedding model id. Uses the default model when empty. |
| `ImageFilePath` | Yes | Image for image generation from source images and for the decisions image input (jpg, jpeg, png, gif, webp; max 20 MB). |
| `ImageGenerationModelId` | No | Image generation model id for prompt-only generation. Uses the default model when empty. |
| `ImageGenerationWithSourceImagesModelId` | Yes | Id of an image generation model that accepts source images. |
| `OcrFilePath` | Yes | Document with text for OCR (pdf, png, jpg, jpeg; max 50 MB). |
| `OcrModelCode` | No | Code of an OCR model. Uses the default model when empty. |
| `OcrUrl` | Yes | Public http(s) URL of a document with text for OCR. |
| `OcrVendor` | No | Vendor of the OCR model. Required when `OcrModelCode` is set. |

Settings can also be provided as environment variables, e.g. `SerenityStar__ApiKey`.

The `*ByFileIdAsync` tests upload the configured file to Serenity Star as a **private** file first. Use non-sensitive test files only: they are sent to the configured AI providers.
