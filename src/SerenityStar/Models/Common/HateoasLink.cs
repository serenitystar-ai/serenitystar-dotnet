namespace SerenityStar.Models.Common
{
    /// <summary>
    /// Represents a HATEOAS link returned alongside a resource, describing a related operation.
    /// </summary>
    public sealed class HateoasLink
    {
        /// <summary>
        /// The relative URL of the related operation.
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// The HTTP method of the related operation (e.g. "GET", "POST", "DELETE").
        /// </summary>
        public string Method { get; set; } = string.Empty;

        /// <summary>
        /// The relation name of the link (e.g. "Update", "Delete", "Next").
        /// </summary>
        public string Rel { get; set; } = string.Empty;
    }
}
