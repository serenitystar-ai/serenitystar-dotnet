using SerenityStar.Models.ImageGeneration;
using System.Threading;
using System.Threading.Tasks;

namespace SerenityStar.AIServices
{
    /// <summary>
    /// Generates images from text prompts.
    /// Obtain an instance with <see cref="Client.SerenityClientAIServicesExtensions.GetImageGenerationClient"/>.
    /// </summary>
    /// <remarks>
    /// This interface is intended to be consumed (and mocked in tests), not implemented: new members may be added in future versions.
    /// </remarks>
    public interface IImageGenerationClient
    {
        /// <summary>
        /// Generates an image from a text prompt.
        /// </summary>
        /// <param name="request">The image generation request.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result including the image URL, usage, and cost.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the request or its prompt is null.</exception>
        /// <exception cref="Exceptions.SerenityApiException">Thrown when the API request fails.</exception>
        Task<ImageGenerationResult> GenerateAsync(GenerateImageReq request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates an image from a text prompt and source images uploaded directly, e.g. to edit one image or combine several.
        /// The file streams are read from their current position and disposed when the request completes.
        /// </summary>
        /// <param name="request">The image generation request containing the source images.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result including the image URL, usage, and cost.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the request, its prompt, or a file's stream or name is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown when no source images are provided.</exception>
        /// <exception cref="Exceptions.SerenityApiException">Thrown when the API request fails.</exception>
        Task<ImageGenerationResult> GenerateWithSourceImagesAsync(GenerateImageWithSourceImagesReq request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates an image from a text prompt and source images previously uploaded to Serenity Star.
        /// </summary>
        /// <param name="request">The image generation request containing the source file ids.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result including the image URL, usage, and cost.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when the request or its prompt is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown when no file ids are provided.</exception>
        /// <exception cref="Exceptions.SerenityApiException">Thrown when the API request fails.</exception>
        Task<ImageGenerationResult> GenerateByFileIdAsync(GenerateImageByFileIdReq request, CancellationToken cancellationToken = default);
    }
}
