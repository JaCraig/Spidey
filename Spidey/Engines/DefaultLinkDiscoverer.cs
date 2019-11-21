/*
Copyright 2017 James Craig

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using Spidey.Engines.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Spidey.Engines
{
    /// <summary>
    /// Default link engine
    /// </summary>
    /// <seealso cref="ILinkDiscoverer"/>
    public class DefaultLinkDiscoverer : ILinkDiscoverer
    {
        /// <summary>
        /// Gets the scheme regex.
        /// </summary>
        /// <value>The scheme regex.</value>
        private static Regex SchemeRegex { get; } = new Regex("^(?<scheme>[^:]*://)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Discovers the urls.
        /// </summary>
        /// <param name="currentDomain">The current domain.</param>
        /// <param name="url">The URL.</param>
        /// <param name="content">The content.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="options">The options.</param>
        /// <returns>The links within the document.</returns>
        public string[] DiscoverUrls(string currentDomain, string url, byte[] content, string contentType, Options options)
        {
            if (contentType.IndexOf("TEXT/HTML", StringComparison.InvariantCultureIgnoreCase) < 0)
                return Array.Empty<string>();
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(Encoding.UTF8.GetString(content));
            return doc
                .DocumentNode
                .SelectNodes("//a[@href]")
                .SelectMany(x => x.Attributes)
                .Select(x => x.Value)
                .Where(x => !string.IsNullOrEmpty(x))
                .Select(x => FixUrl(currentDomain, x, options.UrlReplacementsCompiled))
                .ToArray();
        }

        /// <summary>
        /// Fixes the URL.
        /// </summary>
        /// <param name="currentDomain">The current domain.</param>
        /// <param name="link">The link.</param>
        /// <param name="replacements">The replacements.</param>
        /// <returns>The fixed URL</returns>
        public string FixUrl(string currentDomain, string link, Dictionary<Regex, string> replacements)
        {
            replacements = replacements ?? new Dictionary<Regex, string>();
            link = link.Replace("\\", "/").Trim();
            if (link.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                link = currentDomain + link;
            else if (!SchemeRegex.IsMatch(link))
                link = "http://" + link;
            link = link.Split('#')[0];
            if (link.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                link = link.Remove(link.LastIndexOf('/'), 1);
            link = Uri.UnescapeDataString(link).Trim();
            foreach (var Key in replacements.Keys)
            {
                link = Key.Replace(link, replacements[Key]);
            }
            return Uri.EscapeUriString(link);
        }
    }
}