using SerenityStar.Models.Vision;
using System.Threading;
using System.Threading.Tasks;

namespace SerenityStar.AIServices
{
    /// <summary>
    /// Analyzes images with vision models.
    /// Obtain an instance with <see cref="Client.SerenityClientAIServicesExtensions.GetVisionClient"/>.
    /// </summary>
    /// <remarks>
    /// This interface is intended to be consumed (and mocked in tests), not implemented: new members may be added in future versions.
    /// </remarks>
    public interface IVisionClient
    {
        /// <summary>
        /// Analyzes an image by uploading it directly.
        /// The file stream is read from its current position and disposed when the request completes.
        /// Supported formats: jpg, jpeg, png, gif, webp. Maximum file size: 20 MB.
        /// </summary>
        /// <param name="request">The analysis request containing the file stream, file name, and optional parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result including the model response, usage, and cost.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the request, file stream, or file name is null.</exception>
        /// <exception cref="Exceptions.SerenityApiException">Thrown when the API request fails.</exception>
        Task<VisionResult> AnalyzeAsync(AnalyzeImageReq request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Analyzes an image previously uploaded to Serenity Star.
        /// </summary>
        /// <param name="request">The analysis request containing the file id and optional parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result including the model response, usage, and cost.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the request is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown when the file id is empty.</exception>
        /// <exception cref="Exceptions.SerenityApiException">Thrown when the API request fails.</exception>
        Task<VisionResult> AnalyzeByFileIdAsync(AnalyzeImageByFileIdReq request, CancellationToken cancellationToken = default);
    }
}
