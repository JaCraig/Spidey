using BigBook;
using System.Collections.Concurrent;

namespace Spidey
{
    /// <summary>
    /// Results from the crawl
    /// </summary>
    public class Results
    {
        /// <summary>
        /// Gets or sets the completed urls.
        /// </summary>
        /// <value>The completed urls.</value>
        public ConcurrentBag<string> CompletedURLs { get; } = new ConcurrentBag<string>();

        /// <summary>
        /// Gets or sets the error ur ls.
        /// </summary>
        /// <value>The error ur ls.</value>
        public ConcurrentBag<ErrorItem> ErrorURLs { get; } = new ConcurrentBag<ErrorItem>();

        /// <summary>
        /// Gets or sets the where found.
        /// </summary>
        /// <value>The where found.</value>
        public ListMapping<string, string> WhereFound { get; } = new ListMapping<string, string>();
    }
}