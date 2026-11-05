using System.Collections.Generic;

namespace SerenityStar.Models.Execute
{
    /// <summary>
    /// The user's answer to a question the agent asked (<see cref="UserChoiceQuestion"/>): the options they picked
    /// and/or a free-text answer when none of the options fit.
    /// </summary>
    /// <remarks>
    /// Prefer creating it with <see cref="UserChoiceQuestion.Select"/>, <see cref="UserChoiceQuestion.Answer"/> or
    /// <see cref="UserChoiceQuestion.AnswerOther"/>, which validate it against the question. The API doesn't validate
    /// answers: an answer built by hand is handed to the agent as is.
    /// </remarks>
    public sealed class UserChoiceAnswer
    {
        /// <summary>
        /// The identifier of the question being answered (<see cref="UserChoiceQuestion.Id"/>).
        /// </summary>
        public string QuestionId { get; set; } = string.Empty;

        /// <summary>
        /// The identifiers of the selected options (<see cref="UserChoiceOption.Id"/>).
        /// Only multi-select questions accept more than one.
        /// </summary>
        public List<string> SelectedOptionIds { get; set; } = new List<string>();

        /// <summary>
        /// A free-text answer given when none of the options fit. It can be sent alone or along with selected options.
        /// </summary>
        /// <remarks>
        /// It is stored in the conversation history and can contain personal data: avoid logging it.
        /// </remarks>
        public string? Other { get; set; }
    }
}
