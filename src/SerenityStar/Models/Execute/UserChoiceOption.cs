namespace SerenityStar.Models.Execute
{
    /// <summary>
    /// An option the user can pick when answering a <see cref="UserChoiceQuestion"/>.
    /// </summary>
    public sealed class UserChoiceOption
    {
        /// <summary>
        /// The unique identifier of the option within its question.
        /// It must be sent back exactly as received when the option is selected.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The title of the option, shown to the user.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// An optional description that gives more details about the option.
        /// </summary>
        public string? Description { get; set; }
    }
}
