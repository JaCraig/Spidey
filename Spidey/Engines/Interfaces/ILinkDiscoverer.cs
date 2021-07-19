using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Spidey.Engines.Interfaces
{
    /// <summary>
    /// Link engine interface
    /// </summary>
    public interface ILinkDiscoverer
    {
        /// <summary>
        /// Discovers the urls.
        /// </summary>
        /// <param name="currentDomain">The current domain.</param>
        /// <param name="url">The URL.</param>
        /// <param name="content">The content.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <returns>The links within the document.</returns>
        string[] DiscoverUrls(string currentDomain, string url, byte[] content, string contentType);

        /// <summary>
        /// Fixes the URL.
        /// </summary>
        /// <param name="currentDomain">The current domain.</param>
        /// <param name="link">The link.</param>
        /// <param name="replacements">The replacements.</param>
        /// <returns>Fixed URL</returns>
        string FixUrl(string currentDomain, string link, Dictionary<Regex, string> replacements);

        /// <summary>
        /// Gets the domain.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>The domain of the url.</returns>
        string GetDomain(string url);
    }
}