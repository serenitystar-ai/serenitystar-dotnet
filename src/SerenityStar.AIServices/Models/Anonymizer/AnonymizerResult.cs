using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SerenityStar.Models.Anonymizer
{
    /// <summary>
    /// Represents the result of an anonymization operation.
    /// </summary>
    public sealed class AnonymizerResult
    {
        /// <summary>
        /// The text with every detected entity replaced by its placeholder.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// The detected entities.
        /// </summary>
        public List<AnonymizerSpan> Spans { get; set; } = new List<AnonymizerSpan>();

        /// <summary>
        /// The provider that processed the text.
        /// </summary>
        public string? Provider { get; set; }
    }

    /// <summary>
    /// A sensitive entity detected in the text.
    /// </summary>
    public sealed class AnonymizerSpan
    {
        /// <summary>
        /// The category of the entity (e.g. "private_person").
        /// </summary>
        [JsonPropertyName("entity_group")]
        public string EntityGroup { get; set; } = string.Empty;

        /// <summary>
        /// The base label used for the placeholder.
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// The original text of the entity.
        /// </summary>
        public string Word { get; set; } = string.Empty;

        /// <summary>
        /// The start position of the entity in the original text (inclusive).
        /// </summary>
        public int Start { get; set; }

        /// <summary>
        /// The end position of the entity in the original text (exclusive).
        /// </summary>
        public int End { get; set; }

        /// <summary>
        /// The detection score.
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// The placeholder that replaced the entity in <see cref="AnonymizerResult.Text"/>.
        /// </summary>
        public string Placeholder { get; set; } = string.Empty;
    }
}
