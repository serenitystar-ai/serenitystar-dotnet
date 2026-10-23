using SerenityStar.Models.Ocr;
using System.Threading;
using System.Threading.Tasks;

namespace SerenityStar.AIServices
{
    /// <summary>
    /// Extracts text, tables and images from documents (OCR).
    /// Obtain an instance with <see cref="Client.SerenityClientAIServicesExtensions.GetOcrClient"/>.
    /// </summary>
    /// <remarks>
    /// This interface is intended to be consumed (and mocked in tests), not implemented: new members may be added in future versions.
    /// </remarks>
    public interface IOcrClient
    {
        /// <summary>
        /// Extracts content from documents by uploading them directly.
        /// The file streams are read from their current position and disposed when the request completes.
        /// Supported formats: png, jpg, jpeg, pdf. Maximum size per file: 50 MB.
        /// </summary>
        /// <param name="request">The OCR request containing the files and optional parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of each document, plus usage and cost.</returns>
        /// <exception cref="System.ArgumentException">Thrown when the request has no valid files.</exception>
        /// <exception cref="Exceptions.SerenityApiException">Thrown when the API request fails.</exception>
        Task<OcrExecutionResult> ExtractAsync(OcrReq request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Extracts content from base64-encoded documents.
        /// </summary>
        /// <param name="request">The OCR request containing the base64 files and optional parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of each document, plus usage and cost.</returns>
        /// <exception cref="System.ArgumentException">Thrown when the request has no valid files.</exception>
        /// <exception cref="Exceptions.SerenityApiException">Thrown when the API request fails.</exception>
        Task<OcrExecutionResult> ExtractFromBase64Async(OcrBase64Req request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Extracts content from documents available at public URLs.
        /// </summary>
        /// <param name="request">The OCR request containing the URLs and optional parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of each document, plus usage and cost.</returns>
        /// <exception cref="System.ArgumentException">Thrown when the request has no valid URLs.</exception>
        /// <exception cref="Exceptions.SerenityApiException">Thrown when the API request fails.</exception>
        Task<OcrExecutionResult> ExtractFromUrlsAsync(OcrUrlReq request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Extracts content from documents previously uploaded to Serenity Star.
        /// </summary>
        /// <param name="request">The OCR request containing the file ids and optional parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of each document, plus usage and cost.</returns>
        /// <exception cref="System.ArgumentException">Thrown when the request has no valid file ids.</exception>
        /// <exception cref="Exceptions.SerenityApiException">Thrown when the API request fails.</exception>
        Task<OcrExecutionResult> ExtractByFileIdAsync(OcrByFileIdReq request, CancellationToken cancellationToken = default);
    }
}
