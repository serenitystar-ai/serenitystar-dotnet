using System;

namespace SerenityStar.Models.Files
{
    /// <summary>
    /// A file uploaded to Serenity Star.
    /// </summary>
    public sealed class UploadFileResult
    {
        /// <summary>
        /// The id of the uploaded file. Use it in the methods that accept a file id, e.g. the AI services
        /// <c>*ByFileIdAsync</c> methods or volatile knowledge <c>UploadFromFileForAgentAsync</c>.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The name of the uploaded file.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// The extension of the uploaded file.
        /// </summary>
        public string? Extension { get; set; }
    }
}
