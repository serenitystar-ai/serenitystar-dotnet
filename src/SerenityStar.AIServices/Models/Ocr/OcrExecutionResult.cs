using SerenityStar.Models.AIServices;
using System;
using System.Collections.Generic;

namespace SerenityStar.Models.Ocr
{
    /// <summary>
    /// Represents the result of an OCR operation over one or more documents.
    /// A failure on one document does not abort the others: check <see cref="OcrFileResult.Success"/> per file.
    /// </summary>
    public sealed class OcrExecutionResult
    {
        /// <summary>
        /// The unique identifier of the OCR instance.
        /// </summary>
        public Guid InstanceId { get; set; }

        /// <summary>
        /// The result of each document, in request order.
        /// </summary>
        public List<OcrFileResult> Files { get; set; } = new List<OcrFileResult>();

        /// <summary>
        /// The token counts processed. Null for page-priced models.
        /// </summary>
        public AIServiceTokenUsage? TokenUsage { get; set; }

        /// <summary>
        /// The billed quantities in the units the model is priced by.
        /// </summary>
        public AIServiceUsage? Usage { get; set; }

        /// <summary>
        /// The cost information for this execution in the tenant's currency.
        /// </summary>
        public AIServiceCost? Cost { get; set; }
    }

    /// <summary>
    /// The OCR result of a single document.
    /// </summary>
    public sealed class OcrFileResult
    {
        /// <summary>
        /// The position of the document in the request.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The file name or URL of the document.
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// Whether the document was processed successfully.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The extracted content, when <see cref="Success"/> is true.
        /// </summary>
        public OcrDocument? Result { get; set; }

        /// <summary>
        /// The error message, when <see cref="Success"/> is false.
        /// </summary>
        public string? Error { get; set; }
    }

    /// <summary>
    /// The content extracted from a document.
    /// </summary>
    public sealed class OcrDocument
    {
        /// <summary>
        /// The model that processed the document.
        /// </summary>
        public string? Model { get; set; }

        /// <summary>
        /// The provider that processed the document.
        /// </summary>
        public string? Provider { get; set; }

        /// <summary>
        /// The extracted pages.
        /// </summary>
        public List<OcrPage>? Pages { get; set; }

        /// <summary>
        /// The usage reported by the provider.
        /// </summary>
        public OcrUsageDetails? Usage { get; set; }

        /// <summary>
        /// The markdown of every page joined together.
        /// </summary>
        public string Markdown { get; set; } = string.Empty;
    }

    /// <summary>
    /// A page extracted from a document.
    /// </summary>
    public sealed class OcrPage
    {
        /// <summary>
        /// The position of the page in the document.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The page content as markdown.
        /// </summary>
        public string Markdown { get; set; } = string.Empty;

        /// <summary>
        /// The images found in the page.
        /// </summary>
        public List<OcrImage> Images { get; set; } = new List<OcrImage>();

        /// <summary>
        /// The tables found in the page.
        /// </summary>
        public List<OcrTable> Tables { get; set; } = new List<OcrTable>();

        /// <summary>
        /// The hyperlinks found in the page.
        /// </summary>
        public List<OcrHyperlink> Hyperlinks { get; set; } = new List<OcrHyperlink>();

        /// <summary>
        /// The page header, if detected.
        /// </summary>
        public string? Header { get; set; }

        /// <summary>
        /// The page footer, if detected.
        /// </summary>
        public string? Footer { get; set; }

        /// <summary>
        /// The page dimensions, if reported by the provider.
        /// </summary>
        public OcrPageDimensions? Dimensions { get; set; }

        /// <summary>
        /// The confidence scores, if reported by the provider.
        /// </summary>
        public OcrConfidence? Confidence { get; set; }

        /// <summary>
        /// The layout blocks found in the page.
        /// </summary>
        public List<OcrBlock> Blocks { get; set; } = new List<OcrBlock>();
    }

    /// <summary>
    /// An image found in a page.
    /// </summary>
    public sealed class OcrImage
    {
        /// <summary>
        /// The image identifier, referenced from the page markdown.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The image content, base64 encoded, if returned by the provider.
        /// </summary>
        public string? Base64 { get; set; }

        /// <summary>
        /// The position of the image in the page.
        /// </summary>
        public OcrBoundingBox? BoundingBox { get; set; }
    }

    /// <summary>
    /// The format of a table's content.
    /// </summary>
    public enum OcrTableFormat
    {
        /// <summary>
        /// HTML table.
        /// </summary>
        Html = 0,

        /// <summary>
        /// Markdown table.
        /// </summary>
        Markdown = 1
    }

    /// <summary>
    /// A table found in a page.
    /// </summary>
    public sealed class OcrTable
    {
        /// <summary>
        /// The table content, in <see cref="Format"/>.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// The format of <see cref="Content"/>.
        /// </summary>
        public OcrTableFormat Format { get; set; }

        /// <summary>
        /// The position of the table in the page.
        /// </summary>
        public OcrBoundingBox? BoundingBox { get; set; }
    }

    /// <summary>
    /// A hyperlink found in a page.
    /// </summary>
    public sealed class OcrHyperlink
    {
        /// <summary>
        /// The link target.
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// The link text.
        /// </summary>
        public string? Text { get; set; }

        /// <summary>
        /// The position of the link in the page.
        /// </summary>
        public OcrBoundingBox? BoundingBox { get; set; }
    }

    /// <summary>
    /// A layout block found in a page.
    /// </summary>
    public sealed class OcrBlock
    {
        /// <summary>
        /// The block label (e.g. "title", "paragraph").
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        /// The position of the block in the page.
        /// </summary>
        public OcrBoundingBox? BoundingBox { get; set; }
    }

    /// <summary>
    /// A position in a page, with coordinates normalized from 0 to 1.
    /// </summary>
    public sealed class OcrBoundingBox
    {
        /// <summary>
        /// The left coordinate.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// The top coordinate.
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// The width.
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// The height.
        /// </summary>
        public double Height { get; set; }
    }

    /// <summary>
    /// The dimensions of a page.
    /// </summary>
    public sealed class OcrPageDimensions
    {
        /// <summary>
        /// The page width.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The page height.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// The resolution, if known.
        /// </summary>
        public int? Dpi { get; set; }
    }

    /// <summary>
    /// Confidence scores of the extraction.
    /// </summary>
    public sealed class OcrConfidence
    {
        /// <summary>
        /// The average confidence of the page.
        /// </summary>
        public double? AveragePageScore { get; set; }

        /// <summary>
        /// The lowest confidence in the page.
        /// </summary>
        public double? MinimumPageScore { get; set; }

        /// <summary>
        /// The confidence of each word, if reported.
        /// </summary>
        public List<OcrWordScore>? WordScores { get; set; }
    }

    /// <summary>
    /// The confidence of a single word.
    /// </summary>
    public sealed class OcrWordScore
    {
        /// <summary>
        /// The word.
        /// </summary>
        public string Word { get; set; } = string.Empty;

        /// <summary>
        /// The confidence score.
        /// </summary>
        public double Score { get; set; }
    }

    /// <summary>
    /// The usage reported by the OCR provider.
    /// </summary>
    public sealed class OcrUsageDetails
    {
        /// <summary>
        /// The number of input tokens.
        /// </summary>
        public long? InputTokenCount { get; set; }

        /// <summary>
        /// The number of output tokens.
        /// </summary>
        public long? OutputTokenCount { get; set; }

        /// <summary>
        /// The number of input tokens read from the provider's cache.
        /// </summary>
        public long? CachedInputTokenCount { get; set; }

        /// <summary>
        /// The number of tokens used for reasoning.
        /// </summary>
        public long? ReasoningTokenCount { get; set; }

        /// <summary>
        /// The total number of tokens.
        /// </summary>
        public long? TotalTokenCount { get; set; }

        /// <summary>
        /// Additional provider counts (e.g. "pages").
        /// </summary>
        public Dictionary<string, long>? AdditionalCounts { get; set; }
    }
}
