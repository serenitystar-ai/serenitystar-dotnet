using SerenityStar.Client;
using SerenityStar.Constants;
using SerenityStar.Extensions;
using SerenityStar.Helpers;
using SerenityStar.Models.SpeechGeneration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SerenityStar.AIServices
{
    /// <inheritdoc />
    internal sealed class SpeechGenerationClient : ISpeechGenerationClient
    {
        private const string GeneratePath = "/api/v2/audio/generate";

        private readonly SerenityApiClient _apiClient;

        internal SpeechGenerationClient(SerenityApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        /// <inheritdoc />
        public Task<SpeechGenerationResult> GenerateAsync(
            GenerateSpeechReq request,
            CancellationToken cancellationToken = default)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            request.Validate();

            return AIServiceRequestHelper.PostJsonAsync<SpeechGenerationResult>(
                _apiClient, GeneratePath, CreateBody(request, stream: false), cancellationToken);
        }

        /// <inheritdoc />
        public IAsyncEnumerable<StreamingSpeechGenerationMessage> StreamAsync(
            GenerateSpeechReq request,
            CancellationToken cancellationToken = default)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            request.Validate();

            // Validate eagerly, then defer the request to the iterator.
            return StreamCoreAsync(request, cancellationToken);
        }

        #region Private Methods
        private async IAsyncEnumerable<StreamingSpeechGenerationMessage> StreamCoreAsync(
            GenerateSpeechReq request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            using HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, GeneratePath)
            {
                Content = JsonContent.Create(CreateBody(request, stream: true), options: JsonSerializerOptionsCache.s_camelCaseIgnoreNull)
            };

            using HttpResponseMessage response = await _apiClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            await response.EnsureSerenitySuccessAsync();

            using (Stream stream = await response.Content.ReadAsStreamAsync())
            using (StreamReader reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
                {
                    string? line = await reader.ReadLineAsync();
                    if (string.IsNullOrWhiteSpace(line) || !line.StartsWith("data: "))
                        continue;

                    string data = line.Substring(6).Trim();
                    if (data == "[DONE]")
                        break;

                    StreamingSpeechGenerationMessage? msg = JsonSerializer.Deserialize<StreamingSpeechGenerationMessage>(data, StreamingSpeechGenerationMessageJsonConverter.s_options);
                    if (msg != null)
                        yield return msg;
                }
            }
        }

        private static object CreateBody(GenerateSpeechReq request, bool stream) => new
        {
            request.AIModelId,
            request.ModelCode,
            request.Vendor,
            request.Text,
            request.Voice,
            request.VoiceId,
            request.Speed,
            request.UserIdentifier,
            request.Channel,
            Stream = stream
        };
        #endregion
    }
}
