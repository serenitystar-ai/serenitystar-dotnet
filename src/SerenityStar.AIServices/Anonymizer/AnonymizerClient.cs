using SerenityStar.Client;
using SerenityStar.Helpers;
using SerenityStar.Models.Anonymizer;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SerenityStar.AIServices
{
    /// <inheritdoc />
    internal sealed class AnonymizerClient : IAnonymizerClient
    {
        private readonly SerenityApiClient _apiClient;

        internal AnonymizerClient(SerenityApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        /// <inheritdoc />
        public Task<AnonymizerResult> ObfuscateAsync(
            AnonymizerReq request,
            CancellationToken cancellationToken = default)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            request.Validate();

            return AIServiceRequestHelper.PostJsonAsync<AnonymizerResult>(
                _apiClient, "/api/v2/anonymizer/obfuscate", request, cancellationToken);
        }
    }
}
