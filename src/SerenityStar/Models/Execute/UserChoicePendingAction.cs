using System.Collections.Generic;

namespace SerenityStar.Models.Execute
{
    /// <summary>
    /// Represents a pending action that asks the user to choose from a set of options before the agent continues.
    /// Raised by agents with the Request User Choice skill enabled.
    /// </summary>
    /// <remarks>
    /// The user can answer all, some or none of the <see cref="Questions"/>: the agent continues with whatever it
    /// receives and the unanswered questions stay in the history as context. Build each answer with
    /// <see cref="UserChoiceQuestion.Select"/>, <see cref="UserChoiceQuestion.Answer"/> or
    /// <see cref="UserChoiceQuestion.AnswerOther"/>, and send them back to continue the conversation.
    /// </remarks>
    public sealed class UserChoicePendingAction : PendingAction
    {
        /// <summary>
        /// The type of action that is pending (always "user_choice").
        /// </summary>
        public override string Type => "user_choice";

        /// <summary>
        /// The questions presented to the user, each with its options.
        /// </summary>
        public List<UserChoiceQuestion> Questions { get; set; } = new List<UserChoiceQuestion>();
    }
}
