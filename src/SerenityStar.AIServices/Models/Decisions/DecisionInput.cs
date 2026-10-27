using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SerenityStar.Models.Decisions
{
    /// <summary>
    /// What the decision questions are evaluated against: a text, or an ordered list of items (messages, function
    /// calls and function call outputs), such as an agent trace. Exactly one of <see cref="Text"/> and
    /// <see cref="Items"/> is set.
    /// </summary>
    /// <remarks>
    /// A string converts implicitly, so <c>Input = "The package arrived late."</c> works as before.
    /// Function calls are evidence only and are never executed.
    /// </remarks>
    [JsonConverter(typeof(DecisionInputJsonConverter))]
    public sealed class DecisionInput
    {
        private DecisionInput(string? text, List<DecisionInputItem>? items)
        {
            Text = text;
            Items = items;
        }

        /// <summary>
        /// The input as a single text, or null when the input is a list of items.
        /// </summary>
        public string? Text { get; }

        /// <summary>
        /// The input as an ordered list of items, or null when the input is a text.
        /// </summary>
        public List<DecisionInputItem>? Items { get; }

        /// <summary>
        /// Creates an input from a text.
        /// </summary>
        public static DecisionInput FromText(string text) => new DecisionInput(text, null);

        /// <summary>
        /// Creates an input from an ordered list of items (messages, function calls and function call outputs).
        /// </summary>
        public static DecisionInput FromItems(params DecisionInputItem[] items) => new DecisionInput(null, items.ToList());

        /// <summary>
        /// Creates an input from a text.
        /// </summary>
        public static implicit operator DecisionInput(string text) => FromText(text);

        /// <summary>
        /// Validates the input.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the input is empty or contains null items.</exception>
        internal void Validate()
        {
            if (Items is null ? string.IsNullOrEmpty(Text) : Items.Count == 0)
                throw new ArgumentException("Input requires a non-empty text or at least one item.", "Input");
            if (Items != null && Items.Contains(null!))
                throw new ArgumentException("Input items cannot contain null entries.", "Input");
        }
    }

    /// <summary>
    /// The content of a message or the output of a function call: a text, or a list of parts. Exactly one of
    /// <see cref="Text"/> and <see cref="Parts"/> is set. A string converts implicitly.
    /// </summary>
    [JsonConverter(typeof(DecisionContentJsonConverter))]
    public sealed class DecisionContent
    {
        private DecisionContent(string? text, List<DecisionInputPart>? parts)
        {
            Text = text;
            Parts = parts;
        }

        /// <summary>
        /// The content as a single text, or null when the content is a list of parts.
        /// </summary>
        public string? Text { get; }

        /// <summary>
        /// The content as a list of parts, or null when the content is a text.
        /// </summary>
        public List<DecisionInputPart>? Parts { get; }

        /// <summary>
        /// Creates content from a text.
        /// </summary>
        public static DecisionContent FromText(string text) => new DecisionContent(text, null);

        /// <summary>
        /// Creates content from a list of parts, e.g. a text and an image.
        /// </summary>
        public static DecisionContent FromParts(params DecisionInputPart[] parts) => new DecisionContent(null, parts.ToList());

        /// <summary>
        /// Creates content from a text.
        /// </summary>
        public static implicit operator DecisionContent(string text) => FromText(text);
    }

    /// <summary>
    /// The supported decision input item types.
    /// </summary>
    public static class DecisionInputItemType
    {
        /// <summary>
        /// A message from a <see cref="DecisionMessageRole"/>. The default when the type is not set.
        /// </summary>
        public const string Message = "message";

        /// <summary>
        /// A function (tool) call made by the assistant. Evidence only: it is never executed.
        /// </summary>
        public const string FunctionCall = "function_call";

        /// <summary>
        /// The result of a function call, linked to it by <see cref="DecisionInputItem.CallId"/>.
        /// </summary>
        public const string FunctionCallOutput = "function_call_output";
    }

    /// <summary>
    /// The supported message roles.
    /// </summary>
    public static class DecisionMessageRole
    {
        /// <summary>
        /// The end user. Takes <see cref="DecisionInputPartType.InputText"/> and <see cref="DecisionInputPartType.InputImage"/> parts.
        /// </summary>
        public const string User = "user";

        /// <summary>
        /// Developer instructions. Takes <see cref="DecisionInputPartType.InputText"/> parts.
        /// </summary>
        public const string Developer = "developer";

        /// <summary>
        /// System instructions. Takes <see cref="DecisionInputPartType.InputText"/> parts.
        /// </summary>
        public const string System = "system";

        /// <summary>
        /// The assistant. Takes <see cref="DecisionInputPartType.OutputText"/> and <see cref="DecisionInputPartType.Refusal"/> parts.
        /// </summary>
        public const string Assistant = "assistant";
    }

    /// <summary>
    /// The supported content part types.
    /// </summary>
    public static class DecisionInputPartType
    {
        /// <summary>
        /// A text sent by the user, developer or system, or returned by a function call.
        /// </summary>
        public const string InputText = "input_text";

        /// <summary>
        /// An image sent by the user or returned by a function call, as a base64 data URL.
        /// </summary>
        public const string InputImage = "input_image";

        /// <summary>
        /// A text written by the assistant.
        /// </summary>
        public const string OutputText = "output_text";

        /// <summary>
        /// A refusal written by the assistant. Not allowed in function call outputs.
        /// </summary>
        public const string Refusal = "refusal";
    }

    /// <summary>
    /// The supported image detail levels.
    /// </summary>
    public static class DecisionImageDetail
    {
        /// <summary>
        /// Low detail.
        /// </summary>
        public const string Low = "low";

        /// <summary>
        /// High detail.
        /// </summary>
        public const string High = "high";

        /// <summary>
        /// Lets the model choose the detail level.
        /// </summary>
        public const string Auto = "auto";

        /// <summary>
        /// The image at its original resolution.
        /// </summary>
        public const string Original = "original";
    }

    /// <summary>
    /// An item of a decision input: a message, a function call or a function call output. Each type uses its own
    /// properties; use the factory methods to create items with the right ones.
    /// </summary>
    public sealed class DecisionInputItem
    {
        /// <summary>
        /// The item type. See <see cref="DecisionInputItemType"/>. A message when not set.
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// A message's author. See <see cref="DecisionMessageRole"/>.
        /// </summary>
        public string? Role { get; set; }

        /// <summary>
        /// An assistant message's phase: <c>commentary</c> or <c>final_answer</c>.
        /// </summary>
        public string? Phase { get; set; }

        /// <summary>
        /// A message's content: a text, or a list of parts.
        /// </summary>
        public DecisionContent? Content { get; set; }

        /// <summary>
        /// The id of a function call or function call output.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Links a function call output to its function call.
        /// </summary>
        public string? CallId { get; set; }

        /// <summary>
        /// A function call's namespace.
        /// </summary>
        public string? Namespace { get; set; }

        /// <summary>
        /// The called function's name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// The function call's arguments, usually a JSON object. Sent to the model as is.
        /// </summary>
        public string? Arguments { get; set; }

        /// <summary>
        /// A function call's result: a text, or a list of <see cref="DecisionInputPartType.InputText"/> and
        /// <see cref="DecisionInputPartType.InputImage"/> parts.
        /// </summary>
        public DecisionContent? Output { get; set; }

        /// <summary>
        /// A function call or output's status: <c>in_progress</c>, <c>completed</c> or <c>incomplete</c>.
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Creates a message.
        /// </summary>
        /// <param name="role">The author. See <see cref="DecisionMessageRole"/>.</param>
        /// <param name="content">The content: a text, or <see cref="DecisionContent.FromParts"/>.</param>
        public static DecisionInputItem CreateMessage(string role, DecisionContent content) => new DecisionInputItem
        {
            Type = DecisionInputItemType.Message,
            Role = role,
            Content = content
        };

        /// <summary>
        /// Creates a function call made by the assistant.
        /// </summary>
        /// <param name="callId">The id that links the call to its output.</param>
        /// <param name="name">The called function's name.</param>
        /// <param name="arguments">The arguments, usually a JSON object.</param>
        public static DecisionInputItem CreateFunctionCall(string callId, string name, string arguments) => new DecisionInputItem
        {
            Type = DecisionInputItemType.FunctionCall,
            CallId = callId,
            Name = name,
            Arguments = arguments
        };

        /// <summary>
        /// Creates the output of a function call.
        /// </summary>
        /// <param name="callId">The id of the function call this output belongs to.</param>
        /// <param name="output">The result: a text, or <see cref="DecisionContent.FromParts"/>.</param>
        public static DecisionInputItem CreateFunctionCallOutput(string callId, DecisionContent output) => new DecisionInputItem
        {
            Type = DecisionInputItemType.FunctionCallOutput,
            CallId = callId,
            Output = output
        };
    }

    /// <summary>
    /// A content part: a text, an image or a refusal. Use the factory methods to create parts with the right properties.
    /// </summary>
    public sealed class DecisionInputPart
    {
        /// <summary>
        /// The part type. See <see cref="DecisionInputPartType"/>.
        /// </summary>
        public string Type { get; set; } = null!;

        /// <summary>
        /// The text of <see cref="DecisionInputPartType.InputText"/> and <see cref="DecisionInputPartType.OutputText"/> parts.
        /// </summary>
        public string? Text { get; set; }

        /// <summary>
        /// The image of an <see cref="DecisionInputPartType.InputImage"/> part, as a base64 data URL
        /// (e.g. <c>data:image/png;base64,iVBORw0…</c>). HTTP(S) URLs are not supported.
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// The image detail level. See <see cref="DecisionImageDetail"/>.
        /// </summary>
        public string? Detail { get; set; }

        /// <summary>
        /// The message of a <see cref="DecisionInputPartType.Refusal"/> part.
        /// </summary>
        public string? Refusal { get; set; }

        /// <summary>
        /// Creates a text part sent by the user, developer or system, or returned by a function call.
        /// </summary>
        public static DecisionInputPart CreateInputText(string text) => new DecisionInputPart
        {
            Type = DecisionInputPartType.InputText,
            Text = text
        };

        /// <summary>
        /// Creates an image part from a base64 data URL (e.g. <c>data:image/png;base64,iVBORw0…</c>).
        /// </summary>
        /// <param name="dataUrl">The base64 data URL. HTTP(S) URLs are not supported.</param>
        /// <param name="detail">Optional detail level. See <see cref="DecisionImageDetail"/>.</param>
        public static DecisionInputPart CreateInputImage(string dataUrl, string? detail = null) => new DecisionInputPart
        {
            Type = DecisionInputPartType.InputImage,
            ImageUrl = dataUrl,
            Detail = detail
        };

        /// <summary>
        /// Creates an image part from the image bytes.
        /// </summary>
        /// <param name="image">The image bytes.</param>
        /// <param name="mediaType">The image media type, e.g. <c>image/png</c>.</param>
        /// <param name="detail">Optional detail level. See <see cref="DecisionImageDetail"/>.</param>
        public static DecisionInputPart CreateInputImage(byte[] image, string mediaType, string? detail = null)
        {
            if (image is null)
                throw new ArgumentNullException(nameof(image));
            if (string.IsNullOrEmpty(mediaType))
                throw new ArgumentNullException(nameof(mediaType));

            return CreateInputImage($"data:{mediaType};base64,{Convert.ToBase64String(image)}", detail);
        }

        /// <summary>
        /// Creates a text part written by the assistant.
        /// </summary>
        public static DecisionInputPart CreateOutputText(string text) => new DecisionInputPart
        {
            Type = DecisionInputPartType.OutputText,
            Text = text
        };

        /// <summary>
        /// Creates a refusal part written by the assistant.
        /// </summary>
        public static DecisionInputPart CreateRefusal(string refusal) => new DecisionInputPart
        {
            Type = DecisionInputPartType.Refusal,
            Refusal = refusal
        };
    }

    /// <summary>
    /// Writes a <see cref="DecisionInput"/> as a JSON string or array, as the API expects.
    /// </summary>
    internal sealed class DecisionInputJsonConverter : JsonConverter<DecisionInput>
    {
        public override DecisionInput Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.TokenType switch
            {
                JsonTokenType.String => DecisionInput.FromText(reader.GetString()!),
                JsonTokenType.StartArray => DecisionInput.FromItems(JsonSerializer.Deserialize<DecisionInputItem[]>(ref reader, options)!),
                _ => throw new JsonException("The input must be a string or an array.")
            };

        public override void Write(Utf8JsonWriter writer, DecisionInput value, JsonSerializerOptions options)
        {
            if (value.Items is null)
                writer.WriteStringValue(value.Text);
            else
                JsonSerializer.Serialize(writer, value.Items, options);
        }
    }

    /// <summary>
    /// Writes a <see cref="DecisionContent"/> as a JSON string or array, as the API expects.
    /// </summary>
    internal sealed class DecisionContentJsonConverter : JsonConverter<DecisionContent>
    {
        public override DecisionContent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.TokenType switch
            {
                JsonTokenType.String => DecisionContent.FromText(reader.GetString()!),
                JsonTokenType.StartArray => DecisionContent.FromParts(JsonSerializer.Deserialize<DecisionInputPart[]>(ref reader, options)!),
                _ => throw new JsonException("The content must be a string or an array.")
            };

        public override void Write(Utf8JsonWriter writer, DecisionContent value, JsonSerializerOptions options)
        {
            if (value.Parts is null)
                writer.WriteStringValue(value.Text);
            else
                JsonSerializer.Serialize(writer, value.Parts, options);
        }
    }
}
