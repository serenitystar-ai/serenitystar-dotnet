using SerenityStar.Models.AIServices;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SerenityStar.Models.Decisions
{
    /// <summary>
    /// Represents the result of a decision evaluation.
    /// </summary>
    public sealed class DecisionResult
    {
        /// <summary>
        /// The unique identifier of the decision instance.
        /// </summary>
        public Guid InstanceId { get; set; }

        /// <summary>
        /// The answers, in question order. Use pattern matching on the derived types
        /// (<see cref="PredicateDecisionAnswer"/>, <see cref="ChoiceDecisionAnswer"/>, <see cref="ScoreDecisionAnswer"/>,
        /// <see cref="RefusalDecisionAnswer"/>) to read each answer.
        /// </summary>
        public List<DecisionAnswer> Answers { get; set; } = new List<DecisionAnswer>();

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
    /// Base class for the answer to one decision question.
    /// </summary>
    public abstract class DecisionAnswer
    {
        /// <summary>
        /// The type of the answer.
        /// </summary>
        public abstract string Type { get; }

        /// <summary>
        /// The name of the question this answers.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// The answer to a predicate question.
    /// </summary>
    public sealed class PredicateDecisionAnswer : DecisionAnswer
    {
        /// <inheritdoc />
        public override string Type => DecisionQuestionType.Predicate;

        /// <summary>
        /// Estimated probability, from 0 to 1, that the condition is true.
        /// </summary>
        public double Probability { get; set; }
    }

    /// <summary>
    /// The answer to a choice question.
    /// </summary>
    public sealed class ChoiceDecisionAnswer : DecisionAnswer
    {
        /// <inheritdoc />
        public override string Type => DecisionQuestionType.Choice;

        /// <summary>
        /// The chosen option value.
        /// </summary>
        public string Choice { get; set; } = string.Empty;

        /// <summary>
        /// The probability of each option.
        /// </summary>
        public List<DecisionChoiceProbability> Probabilities { get; set; } = new List<DecisionChoiceProbability>();

        /// <summary>
        /// The confidence of the answer.
        /// </summary>
        public double Confidence { get; set; }
    }

    /// <summary>
    /// The answer to a score question.
    /// </summary>
    public sealed class ScoreDecisionAnswer : DecisionAnswer
    {
        /// <inheritdoc />
        public override string Type => DecisionQuestionType.Score;

        /// <summary>
        /// Probability-weighted average of the 0-based level indices, so it can fall between levels.
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// The probability of each level.
        /// </summary>
        public List<DecisionScoreProbability> Probabilities { get; set; } = new List<DecisionScoreProbability>();

        /// <summary>
        /// The confidence of the answer.
        /// </summary>
        public double Confidence { get; set; }
    }

    /// <summary>
    /// The model declined to answer this question. Other questions in the same request can still be answered.
    /// </summary>
    public sealed class RefusalDecisionAnswer : DecisionAnswer
    {
        /// <inheritdoc />
        public override string Type => "refusal";
    }

    /// <summary>
    /// Represents an answer whose <c>type</c> is not recognized by this SDK version.
    /// This is returned (instead of failing) when the API introduces a new answer type.
    /// </summary>
    public sealed class UnsupportedDecisionAnswer : DecisionAnswer
    {
        /// <inheritdoc />
        public override string Type => "unsupported";

        /// <summary>
        /// The original, unrecognized <c>type</c> value reported by the API.
        /// </summary>
        public string OriginalType { get; set; } = string.Empty;

        /// <summary>
        /// The raw JSON payload of the answer, so consumers can inspect it if needed.
        /// This is a detached copy, safe to read after deserialization completes.
        /// </summary>
        public JsonElement? RawData { get; set; }
    }

    /// <summary>
    /// The probability of a choice option.
    /// </summary>
    public sealed class DecisionChoiceProbability
    {
        /// <summary>
        /// The option value.
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// The probability, from 0 to 1.
        /// </summary>
        public double Probability { get; set; }
    }

    /// <summary>
    /// The probability of a score level.
    /// </summary>
    public sealed class DecisionScoreProbability
    {
        /// <summary>
        /// The 0-based index of the level.
        /// </summary>
        public int Value { get; set; }

        /// <summary>
        /// The level label.
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// The probability, from 0 to 1.
        /// </summary>
        public double Probability { get; set; }
    }
}
