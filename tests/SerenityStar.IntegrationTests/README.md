# Integration Tests

## Configuration

To run the integration tests, you need to:

1. Create an `appsettings.Development.json` file in this directory
2. Add your API key to the file using this format:

```json
{
  "ApiKey": "your-api-key-here"
}

3. If needed, change the code/endpoint of your agents

## Permissions

Most tests only need an API key with the **Execution** permission on the configured agents.

The `GetAllFeedback_*` tests in `MessageFeedbackIntegrationTests` — and
`SubmitFeedback_ResubmittedWithoutComment_ShouldClearTheComment`, which reads the stored feedback
back — call `GetMessageFeedbackAsync` on Assistants/Copilots, which requires the **Audit** permission on
the agent. They fail with an `HttpRequestException` (403) when the configured key lacks it.

That endpoint returns end-user conversation content and user identifiers, so point these tests at a
non-production tenant.
