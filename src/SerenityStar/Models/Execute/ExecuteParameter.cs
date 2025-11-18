using System;

namespace SerenityStar.Models.Execute
{
    /// <summary>
    /// Represents a parameter for agent execution.
    /// </summary>
    public class ExecuteParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExecuteParameter"/> class.
        /// </summary>
        /// <param name="key">The key of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        public ExecuteParameter(string key, object value)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public object Value { get; }
    }
}