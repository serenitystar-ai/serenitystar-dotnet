using System;
using System.Collections.Generic;

namespace SerenityStar.Models.Decisions
{
    /// <summary>
    /// Request for evaluating an input against one or more questions (predicates, choices or scores).
    /// </summary>
    /// <remarks>
    /// A model is required: set <see cref="AIModelId"/>, or both <see cref="ModelCode"/> and <see cref="Vendor"/>.
    /// </remarks>
    public sealed class DecisionReq
    {
        /// <summary>
        /// The id of the decision AI Model to use. Do not combine with <see cref="ModelCode"/> and <see cref="Vendor"/>.
        /// </summary>
        public Guid? AIModelId { get; set; }

        /// <summary>
        /// The code of the decision model to use. Must be combined with <see cref="Vendor"/>.
        /// </summary>
        public string? ModelCode { get; set; }

        /// <summary>
        /// The vendor of the model identified by <see cref="ModelCode"/>.
        /// </summary>
        public string? Vendor { get; set; }

        /// <summary>
        /// What the questions are evaluated against: a text (a string converts implicitly), or an ordered list of
        /// messages, function calls and function call outputs, such as an agent trace, created with
        /// <see cref="DecisionInput.FromItems"/>. User messages can include images.
        /// </summary>
        public DecisionInput Input { get; set; } = null!;

        /// <summary>
        /// The questions to answer about the input. At least one is required and names must be unique.
        /// </summary>
        public List<DecisionQuestion> Questions { get; set; } = new List<DecisionQuestion>();

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
        /// <exception cref="ArgumentNullException">Thrown when Input is null.</exception>
        /// <exception cref="ArgumentException">Thrown when Input is empty or contains null items, or no questions are provided or any is null.</exception>
        public void Validate()
        {
            if (Input is null)
                throw new ArgumentNullException(nameof(Input), "Input is required for a decision.");

            Input.Validate();

            if (Questions is null || Questions.Count == 0 || Questions.Contains(null!))
                throw new ArgumentException("At least one non-null question is required for a decision.", nameof(Questions));
        }
    }

    /// <summary>
    /// The supported decision question types.
    /// </summary>
    public static class DecisionQuestionType
    {
        /// <summary>
        /// A yes/no question. Answered with a probability.
        /// </summary>
        public const string Predicate = "predicate";

        /// <summary>
        /// A question with a closed set of options (at least 2). Answered with the chosen option.
        /// </summary>
        public const string Choice = "choice";

        /// <summary>
        /// A question rated on ordered levels (at least 2, lowest to highest). Answered with a score.
        /// </summary>
        public const string Score = "score";
    }

    /// <summary>
    /// A question to answer about the decision input.
    /// </summary>
    public sealed class DecisionQuestion
    {
        /// <summary>
        /// The question type. See <see cref="DecisionQuestionType"/>.
        /// </summary>
        public string Type { get; set; } = null!;

        /// <summary>
        /// The unique name of the question, used to match its answer.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// The instructions describing what to decide.
        /// </summary>
        public string Instructions { get; set; } = null!;

        /// <summary>
        /// The options for <see cref="DecisionQuestionType.Choice"/> questions (at least 2, unique values).
        /// </summary>
        public List<DecisionChoice>? Choices { get; set; }

        /// <summary>
        /// The levels for <see cref="DecisionQuestionType.Score"/> questions (at least 2, ordered lowest to highest).
        /// </summary>
        public List<DecisionScoreLevel>? Levels { get; set; }
    }

    /// <summary>
    /// An option of a choice question.
    /// </summary>
    public sealed class DecisionChoice
    {
        /// <summary>
        /// The option value, returned in the answer.
        /// </summary>
        public string Value { get; set; } = null!;

        /// <summary>
        /// Optional description of when this option applies.
        /// </summary>
        public string? Description { get; set; }
    }

    /// <summary>
    /// A level of a score question.
    /// </summary>
    public sealed class DecisionScoreLevel
    {
        /// <summary>
        /// The level label.
        /// </summary>
        public string Label { get; set; } = null!;

        /// <summary>
        /// Optional description of the level.
        /// </summary>
        public string? Description { get; set; }
    }
}
