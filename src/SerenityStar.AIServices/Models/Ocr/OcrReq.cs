using SerenityStar.Models.AIServices;
using System;
using System.Collections.Generic;

namespace SerenityStar.Models.Ocr
{
    /// <summary>
    /// Base class for OCR requests. Holds the options shared by every document source.
    /// </summary>
    /// <remarks>
    /// The model is optional: set <see cref="AIModelId"/>, or both <see cref="ModelCode"/> and <see cref="Vendor"/>,
    /// or neither to use the default OCR model.
    /// </remarks>
    public abstract class OcrReqBase
    {
        private protected OcrReqBase()
        {
        }

        /// <summary>
        /// The id of the OCR AI Model to use. Do not combine with <see cref="ModelCode"/> and <see cref="Vendor"/>.
        /// </summary>
        public Guid? AIModelId { get; set; }

        /// <summary>
        /// The code of the OCR model to use. Must be combined with <see cref="Vendor"/>.
        /// </summary>
        public string? ModelCode { get; set; }

        /// <summary>
        /// The vendor of the model identified by <see cref="ModelCode"/> (e.g. "Mistral").
        /// </summary>
        public string? Vendor { get; set; }

        /// <summary>
        /// Optional instructions to guide the extraction.
        /// </summary>
        public string? Instructions { get; set; }

        /// <summary>
        /// Optional user identifier for tracking and cost management purposes.
        /// </summary>
        public string? UserIdentifier { get; set; }

        /// <summary>
        /// The channel used in the execution.
        /// </summary>
        public string? Channel { get; set; }

        /// <summary>
        /// Validates the request parameters.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when no documents are provided or a document is invalid.</exception>
        public abstract void Validate();
    }

    /// <summary>
    /// Request for extracting text from documents by uploading them directly.
    /// </summary>
    public sealed class OcrReq : OcrReqBase
    {
        /// <summary>
        /// The documents to process. At least one is required.
        /// </summary>
        public List<AIServiceFile> Files { get; set; } = new List<AIServiceFile>();

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">Thrown when a file stream or file name is null.</exception>
        public override void Validate()
        {
            if (Files is null || Files.Count == 0)
                throw new ArgumentException("At least one file is required for OCR.", nameof(Files));

            foreach (AIServiceFile file in Files)
            {
                if (file is null)
                    throw new ArgumentException("Files cannot contain null entries.", nameof(Files));
                file.Validate();
            }
        }
    }

    /// <summary>
    /// Request for extracting text from base64-encoded documents.
    /// </summary>
    public sealed class OcrBase64Req : OcrReqBase
    {
        /// <summary>
        /// The documents to process. At least one is required.
        /// </summary>
        public List<OcrBase64File> Base64Files { get; set; } = new List<OcrBase64File>();

        /// <inheritdoc />
        public override void Validate()
        {
            if (Base64Files is null || Base64Files.Count == 0)
                throw new ArgumentException("At least one base64 file is required for OCR.", nameof(Base64Files));

            foreach (OcrBase64File file in Base64Files)
            {
                if (file is null || string.IsNullOrWhiteSpace(file.ContentBase64) || string.IsNullOrWhiteSpace(file.MimeType))
                    throw new ArgumentException("Every base64 file requires ContentBase64 and MimeType.", nameof(Base64Files));
            }
        }
    }

    /// <summary>
    /// A base64-encoded document to process with OCR.
    /// </summary>
    public sealed class OcrBase64File
    {
        /// <summary>
        /// The raw base64 content, without a data-URI prefix.
        /// </summary>
        public string ContentBase64 { get; set; } = null!;

        /// <summary>
        /// The MIME type of the content (e.g. "application/pdf", "image/png").
        /// </summary>
        public string MimeType { get; set; } = null!;

        /// <summary>
        /// Optional file name, used to identify the document in the result.
        /// </summary>
        public string? FileName { get; set; }
    }

    /// <summary>
    /// Request for extracting text from documents available at public URLs.
    /// </summary>
    public sealed class OcrUrlReq : OcrReqBase
    {
        /// <summary>
        /// The absolute http or https URLs of the documents to process. At least one is required.
        /// </summary>
        public List<string> Urls { get; set; } = new List<string>();

        /// <inheritdoc />
        public override void Validate()
        {
            if (Urls is null || Urls.Count == 0)
                throw new ArgumentException("At least one URL is required for OCR.", nameof(Urls));

            foreach (string url in Urls)
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri)
                    || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                    throw new ArgumentException($"'{url}' is not an absolute http or https URL.", nameof(Urls));
            }
        }
    }

    /// <summary>
    /// Request for extracting text from documents previously uploaded to Serenity Star.
    /// </summary>
    public sealed class OcrByFileIdReq : OcrReqBase
    {
        /// <summary>
        /// The ids of the uploaded documents. At least one is required.
        /// </summary>
        public List<Guid> FileIds { get; set; } = new List<Guid>();

        /// <inheritdoc />
        public override void Validate()
        {
            if (FileIds is null || FileIds.Count == 0 || FileIds.Contains(Guid.Empty))
                throw new ArgumentException("At least one non-empty file id is required for OCR.", nameof(FileIds));
        }
    }
}
