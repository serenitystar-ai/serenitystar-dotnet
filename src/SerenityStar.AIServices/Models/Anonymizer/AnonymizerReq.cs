using System;
using System.Collections.Generic;

namespace SerenityStar.Models.Anonymizer
{
    /// <summary>
    /// Request for detecting and obfuscating sensitive data (PII) in a text.
    /// </summary>
    public sealed class AnonymizerReq
    {
        /// <summary>
        /// Optional obfuscation provider. Defaults to "OpenAIPrivacyFilter".
        /// </summary>
        public string? Vendor { get; set; }

        /// <summary>
        /// The text to obfuscate.
        /// </summary>
        public string Inputs { get; set; } = null!;

        /// <summary>
        /// Optional categories to detect. Empty means every category supported by the provider
        /// (for OpenAIPrivacyFilter: private_person, private_address, private_email, private_phone,
        /// private_date, account_number, private_url, secret).
        /// </summary>
        public List<string>? Categories { get; set; }

        /// <summary>
        /// Optional per-category options.
        /// </summary>
        public List<AnonymizerCategoryOption>? CategoryOptions { get; set; }

        /// <summary>
        /// Validates the request parameters.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when Inputs is null or blank.</exception>
        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(Inputs))
                throw new ArgumentNullException(nameof(Inputs), "Inputs is required for anonymization.");
        }
    }

    /// <summary>
    /// Options for a single anonymizer category.
    /// </summary>
    public sealed class AnonymizerCategoryOption
    {
        /// <summary>
        /// The category these options apply to (e.g. "private_person").
        /// </summary>
        public string Category { get; set; } = null!;

        /// <summary>
        /// Optional base label for the placeholders (e.g. "Person" produces "Person-1").
        /// </summary>
        public string? Key { get; set; }

        /// <summary>
        /// Optional minimum detection score, from 0 to 1.
        /// </summary>
        public double? Threshold { get; set; }
    }
}
