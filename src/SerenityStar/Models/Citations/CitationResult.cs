using System;
using System.Text.Json.Serialization;

namespace SerenityStar.Models.Citations
{
    /// <summary>
    /// A citation that links a span of the agent's response back to the source
    /// knowledge section (or web result) it was grounded on.
    /// </summary>
    public class CitationResult
    {
        /// <summary>
        /// The 1-based index of the knowledge section this citation refers to.
        /// </summary>
        public int CitationIndex { get; set; }

        /// <summary>
        /// Absolute character position (0-based) in the accumulated response where the
        /// cited text begins (after stripping all citation markers).
        /// </summary>
        public int StartIndex { get; set; }

        /// <summary>
        /// Absolute character position (0-based, exclusive) in the accumulated response
        /// where the cited text ends.
        /// </summary>
        public int EndIndex { get; set; }

        /// <summary>
        /// Relevance score of the source section (0–1).
        /// </summary>
        public double Relevance { get; set; }

        /// <summary>
        /// Source document metadata for this citation. Null if no matching knowledge section was found.
        /// </summary>
        public SourceInfo? Source { get; set; }

        /// <summary>
        /// The verbatim content of the knowledge section identified by <see cref="CitationIndex"/>.
        /// </summary>
        public string? CitedText { get; set; }

        /// <summary>
        /// Source document metadata for a citation. The concrete type depends on where the
        /// cited content came from (a knowledge file, an ingested website, or a web search result).
        /// </summary>
        [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
        [JsonDerivedType(typeof(KnowledgeFile), "knowledge_file")]
        [JsonDerivedType(typeof(KnowledgeWebsite), "knowledge_website")]
        [JsonDerivedType(typeof(WebSearch), "web_search")]
        public abstract class SourceInfo
        {
            /// <summary>
            /// Citation sourced from a knowledge base file (paginated document).
            /// </summary>
            public sealed class KnowledgeFile : SourceInfo
            {
                /// <summary>
                /// The original filename of the source document.
                /// </summary>
                public string? FileName { get; set; }

                /// <summary>
                /// The ID of the knowledge file version that contains the cited section.
                /// </summary>
                public Guid KnowledgeFileVersionId { get; set; }

                /// <summary>
                /// The embedding store key that uniquely identifies the cited section within the knowledge file version.
                /// </summary>
                public string? SectionId { get; set; }

                /// <summary>
                /// The page range within the source document where the cited section appears (e.g. "3-5").
                /// </summary>
                public string? PageRange { get; set; }

                /// <summary>
                /// Whether the cited file can be downloaded by authorized users of the agent.
                /// </summary>
                public bool IsDownloadable { get; set; }
            }

            /// <summary>
            /// Citation sourced from a knowledge base entry ingested from a website.
            /// </summary>
            public sealed class KnowledgeWebsite : SourceInfo
            {
                /// <summary>
                /// The ID of the knowledge file version that contains the cited section.
                /// </summary>
                public Guid KnowledgeFileVersionId { get; set; }

                /// <summary>
                /// The embedding store key that uniquely identifies the cited section within the knowledge file version.
                /// </summary>
                public string? SectionId { get; set; }

                /// <summary>
                /// The URL of the website from which the cited section was extracted.
                /// </summary>
                public string? Website { get; set; }
            }

            /// <summary>
            /// Citation sourced from a result returned by the WebSearch skill.
            /// </summary>
            public sealed class WebSearch : SourceInfo
            {
                /// <summary>
                /// The URL of the web page that backs the citation.
                /// </summary>
                public string? Url { get; set; }

                /// <summary>
                /// The title of the web page that backs the citation.
                /// </summary>
                public string? Title { get; set; }
            }
        }
    }
}
