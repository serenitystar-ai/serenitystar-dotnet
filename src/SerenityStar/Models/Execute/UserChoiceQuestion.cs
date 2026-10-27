using System;
using System.Collections.Generic;
using System.Linq;

namespace SerenityStar.Models.Execute
{
    /// <summary>
    /// A question the agent asked the user (<see cref="UserChoicePendingAction"/>), with the options to choose from.
    /// </summary>
    public sealed class UserChoiceQuestion
    {
        /// <summary>
        /// The unique identifier of the question.
        /// It must be sent back exactly as received in the answer.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// A short label for the question, for example to use as its tab title.
        /// </summary>
        public string? Header { get; set; }

        /// <summary>
        /// The question text.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Whether the user can select more than one option.
        /// </summary>
        public bool IsMultiselect { get; set; }

        /// <summary>
        /// The options the user can choose from.
        /// </summary>
        public List<UserChoiceOption> Options { get; set; } = new List<UserChoiceOption>();

        /// <summary>
        /// Creates the answer that selects the given options.
        /// </summary>
        /// <param name="optionIds">The identifiers of the selected options. Only multi-select questions accept more than one.</param>
        /// <returns>The answer to send back to the agent.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when no option is selected, an option doesn't belong to this question, or more than one option is
        /// selected on a single-select question.
        /// </exception>
        public UserChoiceAnswer Select(params string[] optionIds)
        {
            if (optionIds == null || optionIds.Length == 0)
                throw new ArgumentException("At least one option must be selected.", nameof(optionIds));

            return Answer(optionIds, null);
        }

        /// <summary>
        /// Creates the answer with a free-text response, for when none of the options fit.
        /// </summary>
        /// <param name="other">The free-text answer.</param>
        /// <returns>The answer to send back to the agent.</returns>
        /// <exception cref="ArgumentException">Thrown when the text is empty.</exception>
        public UserChoiceAnswer AnswerOther(string other)
        {
            if (string.IsNullOrWhiteSpace(other))
                throw new ArgumentException("The free-text answer can't be empty.", nameof(other));

            return Answer(null, other);
        }

        /// <summary>
        /// Creates the answer that selects the given options and/or adds a free-text response.
        /// </summary>
        /// <param name="optionIds">The identifiers of the selected options, if any. Only multi-select questions accept more than one.</param>
        /// <param name="other">An optional free-text answer, sent along with the selected options.</param>
        /// <returns>The answer to send back to the agent.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when neither options nor text are provided, an option doesn't belong to this question, or more than
        /// one option is selected on a single-select question.
        /// </exception>
        public UserChoiceAnswer Answer(IEnumerable<string>? optionIds, string? other = null)
        {
            List<string> selected = optionIds?.Distinct(StringComparer.Ordinal).ToList() ?? new List<string>();
            string? otherText = string.IsNullOrWhiteSpace(other) ? null : other;

            if (selected.Count == 0 && otherText == null)
                throw new ArgumentException("The answer must select at least one option or include a free-text answer.", nameof(optionIds));

            if (!IsMultiselect && selected.Count > 1)
                throw new ArgumentException($"Question '{Id}' only accepts one option.", nameof(optionIds));

            string? unknown = selected.FirstOrDefault(id => !Options.Any(o => string.Equals(o.Id, id, StringComparison.Ordinal)));
            if (unknown != null)
                throw new ArgumentException($"Option '{unknown}' doesn't belong to question '{Id}'.", nameof(optionIds));

            return new UserChoiceAnswer { QuestionId = Id, SelectedOptionIds = selected, Other = otherText };
        }
    }
}
