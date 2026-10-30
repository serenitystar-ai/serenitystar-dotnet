using SerenityStar.Client;
using SerenityStar.Constants;
using SerenityStar.Extensions;
using SerenityStar.Models.AIServices;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SerenityStar.Helpers
{
    /// <summary>
    /// Internal helpers shared by the AI service clients to build and send requests.
    /// </summary>
    internal static class AIServiceRequestHelper
    {
        /// <summary>
        /// Sends <paramref name="body"/> as JSON (nulls omitted) and deserializes the response.
        /// </summary>
        internal static Task<TResult> PostJsonAsync<TResult>(
            SerenityApiClient apiClient,
            string path,
            object body,
            CancellationToken cancellationToken)
            => PostJsonAsync<TResult>(apiClient, path, body, JsonSerializerOptionsCache.s_camelCase, cancellationToken);

        /// <summary>
        /// Sends <paramref name="body"/> as JSON (nulls omitted) and deserializes the response with the given options.
        /// </summary>
        internal static async Task<TResult> PostJsonAsync<TResult>(
            SerenityApiClient apiClient,
            string path,
            object body,
            JsonSerializerOptions responseOptions,
            CancellationToken cancellationToken)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = JsonContent.Create(body, body.GetType(), options: JsonSerializerOptionsCache.s_camelCaseIgnoreNull)
            };

            using HttpResponseMessage response = await apiClient.SendAsync(request, cancellationToken);

            return await response.ReadSerenityJsonAsync<TResult>(responseOptions, cancellationToken);
        }

        /// <summary>
        /// Sends the multipart form and deserializes the response.
        /// </summary>
        internal static async Task<TResult> PostMultipartAsync<TResult>(
            SerenityApiClient apiClient,
            string path,
            MultipartFormDataContent content,
            CancellationToken cancellationToken)
        {
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, path)
            {
                Content = content
            };

            using HttpResponseMessage response = await apiClient.SendAsync(request, cancellationToken);

            return await response.ReadSerenityJsonAsync<TResult>(cancellationToken);
        }

        /// <summary>
        /// Adds a text field to the form, skipping null or empty values.
        /// </summary>
        internal static void AddField(this MultipartFormDataContent content, string name, string? value)
        {
            if (!string.IsNullOrEmpty(value))
                content.Add(new StringContent(value), name);
        }

        /// <summary>
        /// Adds a file to the form. The content type is inferred from the file extension; unknown extensions are
        /// sent as application/octet-stream (OCR and image generation resolve it from the file name;
        /// transcription forwards it as is).
        /// </summary>
        /// <remarks>
        /// The stream is owned by the request from here on: it is disposed together with the request, as the
        /// other upload helpers of the SDK do.
        /// </remarks>
        internal static void AddFile(this MultipartFormDataContent content, string name, Stream fileStream, string fileName)
        {
            StreamContent fileContent = new(fileStream);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(ContentTypeHelper.GetContentType(fileName));
            content.Add(fileContent, name, fileName);
        }

        /// <summary>
        /// Adds a file to the form.
        /// </summary>
        internal static void AddFile(this MultipartFormDataContent content, string name, AIServiceFile file)
            => content.AddFile(name, file.FileStream, file.FileName);
    }
}
