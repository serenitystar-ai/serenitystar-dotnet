using SerenityStar.Errors.Constants;
using System.Collections.Generic;

namespace SerenityStar.Errors.Models
{
    /// <summary>
    /// The request itself failed validation before the endpoint ran: a missing required field, a wrong type,
    /// or a body that isn't valid JSON (<see cref="SerenityErrorCodes.InputValidationError"/>, HTTP 400).
    /// </summary>
    public sealed class InputValidationError : SerenityApiError
    {
        /// <summary>
        /// The offending request field (in the API's JSON casing, dot-separated for nested members),
        /// mapped to every validation message for that field.
        /// </summary>
        public Dictionary<string, List<string>> Errors { get; set; } = new();
    }
}
