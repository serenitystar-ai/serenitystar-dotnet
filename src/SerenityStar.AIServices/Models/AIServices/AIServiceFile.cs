using System;
using System.IO;

namespace SerenityStar.Models.AIServices
{
    /// <summary>
    /// A file sent to an AI service by uploading it directly.
    /// </summary>
    public sealed class AIServiceFile
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
        /// Validates the file.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when FileStream or FileName is null.</exception>
        public void Validate()
        {
            if (FileStream is null)
                throw new ArgumentNullException(nameof(FileStream), "File stream is required.");
            if (string.IsNullOrEmpty(FileName))
                throw new ArgumentNullException(nameof(FileName), "File name is required.");
        }
    }
}
