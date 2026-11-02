using System;
using System.IO;

namespace SerenityStar.Models.Files
{
    /// <summary>
    /// Request for uploading a file to Serenity Star.
    /// </summary>
    public sealed class UploadFileReq
    {
        /// <summary>
        /// The file stream to upload.
        /// </summary>
        public Stream FileStream { get; set; } = null!;

        /// <summary>
        /// The file name including extension (e.g., "invoice.pdf"). The extension is used to set the content type.
        /// </summary>
        public string FileName { get; set; } = null!;

        /// <summary>
        /// Validates the request parameters.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when FileStream is null, or FileName is null, empty or whitespace.</exception>
        public void Validate()
        {
            if (FileStream is null)
                throw new ArgumentNullException(nameof(FileStream), "File stream is required.");
            if (string.IsNullOrWhiteSpace(FileName))
                throw new ArgumentNullException(nameof(FileName), "File name is required.");
        }
    }
}
