using SerenityStar.Models.Decisions;
using System.Threading;
using System.Threading.Tasks;

namespace SerenityStar.AIServices
{
    /// <summary>
    /// Evaluates inputs against predicate, choice and score questions.
    /// Obtain an instance with <see cref="Client.SerenityClientAIServicesExtensions.GetDecisionClient"/>.
    /// </summary>
    /// <remarks>
    /// This interface is intended to be consumed (and mocked in tests), not implemented: new members may be added in future versions.
    /// </remarks>
    public interface IDecisionClient
    {
        /// <summary>
        /// Evaluates the input and answers every question.
        /// </summary>
        /// <param name="request">The decision request containing the input, questions and model.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The answers, in question order, plus usage and cost.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the request or its input is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown when no questions are provided.</exception>
        /// <exception cref="Exceptions.SerenityApiException">Thrown when the API request fails.</exception>
        Task<DecisionResult> EvaluateAsync(DecisionReq request, CancellationToken cancellationToken = default);
    }
}
