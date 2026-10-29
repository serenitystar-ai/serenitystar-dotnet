using SerenityStar.Models.Files;
using System.Threading;
using System.Threading.Tasks;

namespace SerenityStar.Files
{
    /// <summary>
    /// Uploads files to Serenity Star, to use them later by id.
    /// Obtain an instance with <see cref="Client.SerenityClient.GetFileClient"/>.
    /// </summary>
    /// <remarks>
    /// Uploaded files are not public: they cannot be downloaded anonymously, only through the API with your API key.
    /// This interface is intended to be consumed (and mocked in tests), not implemented: new members may be added in future versions.
    /// </remarks>
    public interface IFileClient
    {
        /// <summary>
        /// Uploads a file.
        /// The file stream is read from its current position and disposed when the request completes.
        /// </summary>
        /// <param name="request">The upload request containing the file stream and file name.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The uploaded file, including its id.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the request, file stream, or file name is null.</exception>
        /// <exception cref="Exceptions.SerenityApiException">Thrown when the API request fails.</exception>
        Task<UploadFileResult> UploadAsync(UploadFileReq request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Uploads a file from a publicly accessible URL. The file is downloaded by Serenity Star.
        /// </summary>
        /// <param name="request">The upload request containing the file URL and optional file name.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The uploaded file, including its id.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the request or file URL is null.</exception>
        /// <exception cref="Exceptions.SerenityApiException">Thrown when the API request fails, e.g. the URL is invalid or the file could not be downloaded.</exception>
        Task<UploadFileResult> UploadFromUrlAsync(UploadFileFromUrlReq request, CancellationToken cancellationToken = default);
    }
}
