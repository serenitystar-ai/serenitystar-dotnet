using System.Text.Json.Serialization;

namespace SerenityStar.Models.MessageFeedback;

public class MessageFeedbackRequest
{
    [JsonPropertyName("feedback")]
    public bool Feedback { get; set; }
}
