namespace SerenityStar.Models.AIServices
{
    /// <summary>
    /// The billed quantities of an AI service execution, in the units the model is priced by
    /// (e.g. tokens, characters, seconds, images or pages).
    /// </summary>
    public sealed class AIServiceUsage
    {
        /// <summary>
        /// The unit of the input quantity (e.g. "tokens", "characters").
        /// </summary>
        public string? InputUnit { get; set; }

        /// <summary>
        /// The billed input quantity, in <see cref="InputUnit"/>.
        /// </summary>
        public int? Input { get; set; }

        /// <summary>
        /// The unit of the output quantity (e.g. "tokens", "images").
        /// </summary>
        public string? OutputUnit { get; set; }

        /// <summary>
        /// The billed output quantity, in <see cref="OutputUnit"/>.
        /// </summary>
        public int? Output { get; set; }
    }

    /// <summary>
    /// The cost of an AI service execution in the tenant's currency.
    /// </summary>
    public sealed class AIServiceCost
    {
        /// <summary>
        /// The total cost in the tenant's currency.
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// The ISO currency code for the tenant's currency.
        /// </summary>
        public string? Currency { get; set; }
    }

    /// <summary>
    /// The cost of a token-priced AI service execution in the tenant's currency.
    /// </summary>
    public sealed class AIServiceTokenCost
    {
        /// <summary>
        /// The cost for prompt tokens in the tenant's currency.
        /// </summary>
        public double Prompt { get; set; }

        /// <summary>
        /// The cost for completion tokens in the tenant's currency.
        /// </summary>
        public double Completion { get; set; }

        /// <summary>
        /// The total cost (prompt + completion) in the tenant's currency.
        /// </summary>
        public double Total { get; set; }

        /// <summary>
        /// The ISO currency code for the tenant's currency.
        /// </summary>
        public string? Currency { get; set; }
    }

    /// <summary>
    /// The token counts processed by an AI service execution.
    /// </summary>
    public sealed class AIServiceTokenUsage
    {
        /// <summary>
        /// The number of tokens in the prompt.
        /// </summary>
        public int PromptTokens { get; set; }

        /// <summary>
        /// The number of tokens generated.
        /// </summary>
        public int CompletionTokens { get; set; }

        /// <summary>
        /// The total number of tokens processed.
        /// </summary>
        public int TotalTokens { get; set; }
    }
}
