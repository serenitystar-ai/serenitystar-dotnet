using SerenityStar.Client;
using SerenityStar.Helpers;
using SerenityStar.Models.Decisions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SerenityStar.AIServices
{
    /// <inheritdoc />
    internal sealed class DecisionClient : IDecisionClient
    {
        private readonly SerenityApiClient _apiClient;

        internal DecisionClient(SerenityApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        /// <inheritdoc />
        public Task<DecisionResult> EvaluateAsync(
            DecisionReq request,
            CancellationToken cancellationToken = default)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            request.Validate();

            return AIServiceRequestHelper.PostJsonAsync<DecisionResult>(
                _apiClient, "/api/v2/decisions", request, DecisionAnswerJsonConverter.s_options, cancellationToken);
        }
    }
}
