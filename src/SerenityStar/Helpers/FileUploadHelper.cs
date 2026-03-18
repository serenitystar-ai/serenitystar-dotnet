using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SerenityStar.Helpers
{
    /// <summary>
    /// Internal helper for uploading files to the Serenity Star API.
    /// </summary>
    internal static class FileUploadHelper
    {
        /// <summary>
        /// Uploads a file and returns the resulting file ID.
        /// </summary>
        internal static async Task<Guid> UploadFileAsync(
            HttpClient httpClient,
            Stream fileStream,
            string fileName,
            CancellationToken cancellationToken = default)
        {
            using MultipartFormDataContent content = new();

            StreamContent fileContent = new(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            content.Add(fileContent, "formFile", fileName);

            HttpResponseMessage response = await httpClient.PostAsync(
                "/api/File/upload/public",
                content,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                string errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(
                    $"File upload failed with status code {response.StatusCode}: {errorContent}");
            }

            string responseBody = await response.Content.ReadAsStringAsync();

            using JsonDocument doc = JsonDocument.Parse(responseBody);
            if (doc.RootElement.TryGetProperty("id", out JsonElement idElement)
                && Guid.TryParse(idElement.GetString(), out Guid fileId))
                return fileId;

            throw new InvalidOperationException($"Unable to parse file ID from upload response: {responseBody}");
        }
    }
}
