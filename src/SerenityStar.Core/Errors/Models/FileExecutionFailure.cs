using System.Collections.Generic;

namespace SerenityStar.Errors.Models
{
    /// <summary>
    /// The failed attempts for one file submitted to OCR.
    /// </summary>
    public sealed class FileExecutionFailure
    {
        /// <summary>
        /// The 0-based position of the file in the request.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The file's name or URL, when the request provided one.
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// Every attempt made for this file, in order.
        /// </summary>
        public List<ExecutionAttempt> Attempts { get; set; } = new();
    }
}
