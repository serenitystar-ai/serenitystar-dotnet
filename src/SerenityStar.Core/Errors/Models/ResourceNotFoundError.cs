using SerenityStar.Errors.Constants;
using System.Collections.Generic;

namespace SerenityStar.Errors.Models
{
    /// <summary>
    /// The requested resource does not exist, or no route matched the request
    /// (<see cref="SerenityErrorCodes.ResourceNotFound"/>, HTTP 404).
    /// </summary>
    public sealed class ResourceNotFoundError : SerenityApiError
    {
        /// <summary>
        /// A single entry whose key identifies the missing resource. See <see cref="ResourceNotFoundKeys"/>.
        /// Null when there is no specific resource to report, such as a request that matched no route.
        /// </summary>
        public Dictionary<string, string>? Errors { get; set; }
    }
}
