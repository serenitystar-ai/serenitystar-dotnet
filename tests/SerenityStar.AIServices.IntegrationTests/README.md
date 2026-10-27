# AI Services Integration Tests

This project has two kinds of tests:

- **`AIServicesOfflineTests`**: run without an API key or network. A fake HTTP handler captures each request and returns a canned response, to check the requests the SDK builds and how it parses responses.
- **`*IntegrationTests`**: run against the real Serenity Star API. They consume credits from the tenant of the configured API key.

## Configuration

Create an `appsettings.Development.json` file in this directory (it is git-ignored) and set every value marked `replace-me` in `appsettings.json`. Empty values and `replace-me` are treated as not configured; a test that needs a missing value fails with a message naming the setting.

Models are selected by code and vendor (`*ModelCode` + `*Vendor`) on the services that support it: decisions. Both values must be set together. The vendor is the name of a Serenity Star vendor, e.g. `OpenAI`, `Anthropic`, `Google` or `Mistral`. Embeddings only accepts a model id (`*ModelId`).

```json
{
  "SerenityStar": {
    "ApiKey": "your-api-key",
    "DecisionModelCode": "your-decision-model-code",
    "DecisionVendor": "OpenAI",
    "ImageFilePath": "C:\\test-files\\image.png"
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
| `ImageFilePath` | Yes | Image for the decisions image input (jpg, jpeg, png, gif, webp; max 20 MB). |

Settings can also be provided as environment variables, e.g. `SerenityStar__ApiKey`.
